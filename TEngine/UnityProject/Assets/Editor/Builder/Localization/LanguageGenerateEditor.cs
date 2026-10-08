using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameConfig;
using GameConfig.language;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Builder
{
    /// <summary>
    /// 多语言 SDF 字体自动生成工具。
    /// 核心思路：以「字体文件」为维度，而非以「语言」为维度。
    /// 一种字体(.ttf)可能被多种语言共用（如中日文共用同一款字体），
    /// 我们只生成一份 SDF 资源，包含所有使用该字体的语言中出现的全部字符。
    /// 最终输出两套资源：普通文本字体 和 设置面板字体（加 _Setting 后缀）。
    /// </summary>
    public static class LanguageGenerateEditor
    {
        #region 可调参数（按需修改）

        /// <summary>字体采样字号（值越大字形越清晰，图集消耗也越大）</summary>
        private const int SamplingPointSize = 60;

        /// <summary>图集内边距（像素），防止 SDF 边缘溢出到相邻字形</summary>
        private const int AtlasPadding = 6;

        /// <summary>SDF 渲染模式</summary>
        private const GlyphRenderMode RenderMode = GlyphRenderMode.SDF8;

        /// <summary>源字体(.ttf)所在目录</summary>
        private const string SourceFontDir = "Assets/AssetArt/Font";

        /// <summary>普通字体 SDF 资源输出目录</summary>
        private const string OutputFontDir = "Assets/AssetRaw/Fonts";

        /// <summary>设置面板字体 SDF 资源输出子目录</summary>
        private const string SettingSubDir = "Setting";

        /// <summary>设置面板字体名称后缀</summary>
        private const string SettingSuffix = "_Setting";

        /// <summary>SDF 字体使用的 Shader 名称</summary>
        private const string SDFShaderName = "TextMeshPro/Distance Field";

        /// <summary>图集最大尺寸（CJK 大字集建议 4096）</summary>
        private const int AtlasMaxSize = 4096;

        #endregion

        // ================================================================
        //  主入口
        // ================================================================

        /// <summary>
        /// 菜单入口：TEngine -> Localization -> 生成语言文件。
        /// 1) 加载配置表
        /// 2) 将语言按「字体文件」分组
        /// 3) 每组收集其涵盖的所有多语言文本的唯一字符
        /// 4) 为每组字体生成两个 TMPro SDF 资源（普通 + 设置面板）
        /// 5) 刷新资源数据库
        /// </summary>
        [MenuItem("TEngine/Localization/生成语言文件", priority = -100)]
        public static void Generate()
        {
            try
            {
                // ---- 1. 加载编辑器配置表 ----
                EditorUtility.DisplayProgressBar("多语言字体生成", "加载配置表 ...", 0f);
                ConfigSystem.Instance.LoadEditor();

                var langTable   = ConfigSystem.Instance.Tables.TbLanguage;
                var contentTable = ConfigSystem.Instance.Tables.TbLanguageContent;

                if (langTable == null || langTable.DataMap.Count == 0)
                {
                    ShowError("未找到语言配置数据，请先执行 TEngine/Luban/转表。");
                    return;
                }
                if (contentTable == null || contentTable.DataMap.Count == 0)
                {
                    ShowError("未找到多语言文本数据，请先执行 TEngine/Luban/转表。");
                    return;
                }

                // ---- 2. 按字体分组语言 ----
                EditorUtility.DisplayProgressBar("多语言字体生成", "按字体分组语言 ...", 0.05f);
                Dictionary<string, List<ConfLanguage>> fontGroups = GroupLanguagesByFont(langTable);

                if (fontGroups.Count == 0)
                {
                    ShowError("所有语言均未配置字体文件名 (Font 字段为空)，无法生成。");
                    return;
                }

                // 构建「设置面板 key」集合（TbLanguage 中定义的 Name）
                HashSet<string> settingKeys = BuildSettingKeySet(langTable);
                // ---- 3 & 4. 逐字体收集字符并生成 SDF 资源 ----
                float totalSteps = fontGroups.Count * 4f;   // 每种字体 × 4（两次收集和两次构建）
                int   step = 0;

                foreach (var kv in fontGroups)
                {
                    string fontFileName    = kv.Key;
                    List<ConfLanguage> langs = kv.Value;

                    // 3a. 收集普通文本字符（排除设置面板 key）
                    step++;
                    float progress = 0.1f + 0.85f * (step / totalSteps);
                    EditorUtility.DisplayProgressBar("多语言字体生成", $"分析字符 [{fontFileName}] 普通文本 ...", progress);

                    HashSet<char> normalChars = CollectCharsForFontGroup(langs, contentTable, settingKeys, settingOnly: false);
                    // 通用UI补充字符按源字体名管理，不扫描文字关卡或拼音。
                    string extraPath = Path.Combine("Assets/Editor/Builder/Localization/AdditionalCharacters",
                        Path.GetFileNameWithoutExtension(fontFileName) + ".txt");
                    if (File.Exists(extraPath))
                        foreach (char c in File.ReadAllText(extraPath))
                            if (!char.IsControl(c)) normalChars.Add(c);
                    EnsureDirectory(OutputFontDir);

                    // 3b. 生成普通字体 SDF 资源
                    step++;
                    progress = 0.1f + 0.85f * (step / totalSteps);
                    EditorUtility.DisplayProgressBar("多语言字体生成", $"生成 [{fontFileName}] SDF 字体 ...", progress);

                    if (normalChars.Count > 0)
                    {
                        BuildFontAsset(fontFileName, normalChars, OutputFontDir, string.Empty);
                    }
                    else
                    {
                        Debug.LogWarning($"[字体生成] [{fontFileName}] 无普通文本字符，跳过。");
                    }

                    // 4a. 收集设置面板文本字符（仅设置面板 key）
                    step++;
                    progress = 0.1f + 0.85f * (step / totalSteps);
                    EditorUtility.DisplayProgressBar("多语言字体生成",
                        $"分析字符 [{fontFileName}] 设置面板 ...", progress);

                    HashSet<char> settingChars = CollectCharsForFontGroup(langs, contentTable, settingKeys, settingOnly: true);

                    // 4b. 生成设置面板字体 SDF 资源
                    step++;
                    progress = 0.1f + 0.85f * (step / totalSteps);
                    EditorUtility.DisplayProgressBar("多语言字体生成",
                        $"生成 [{fontFileName}] 设置面板 SDF 字体 ...", progress);

                    string settingDir = Path.Combine(OutputFontDir, SettingSubDir);
                    if (settingChars.Count > 0)
                    {
                        EnsureDirectory(settingDir);
                        BuildFontAsset(fontFileName, settingChars, settingDir, SettingSuffix);
                    }
                    else
                    {
                        Debug.LogWarning($"[字体生成] [{fontFileName}] 无设置面板字符，跳过。");
                    }
                }

                // ---- 5. 保存并刷新 ----
                EditorUtility.DisplayProgressBar("多语言字体生成", "刷新资源数据库 ...", 0.98f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                Debug.Log("[字体生成] 全部 SDF 字体生成完毕！");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[字体生成] 异常:\n{ex}");
                if (Application.isBatchMode) throw;
                EditorUtility.DisplayDialog("字体生成失败", ex.Message, "确定");
            }
        }

        // ================================================================
        //  字体分组
        // ================================================================

        /// <summary>
        /// 将 TbLanguage 中所有语言按 Font 字段分组。
        /// 多种语言可能共用同一款 .ttf 字体（如中、日文共用），
        /// 我们只为每个字体文件生成一份 SDF 资源。
        /// </summary>
        /// <returns>Key = 字体文件名（如 "AaFengKuangYuanShiRen.ttf"），Value = 使用该字体的语言列表</returns>
        private static Dictionary<string, List<ConfLanguage>> GroupLanguagesByFont(
            TbLanguage langTable)
        {
            var groups = new Dictionary<string, List<ConfLanguage>>();

            foreach (var kv in langTable.DataMap)
            {
                ConfLanguage cfg = kv.Value;
                string font = cfg.Font;

                // 跳过未配置字体的语言
                if (string.IsNullOrEmpty(font))
                {
                    Debug.LogWarning($"[字体分组] 语言 [{cfg.Name}] 未配置 Font 字段，跳过。");
                    continue;
                }

                if (!groups.TryGetValue(font, out var list))
                {
                    list = new List<ConfLanguage>();
                    groups[font] = list;
                }

                list.Add(cfg);
            }

            // 日志：输出分组结果
            foreach (var kv in groups)
            {
                var names = new List<string>();
                foreach (var l in kv.Value) names.Add(l.Name);
                Debug.Log($"[字体分组] 字体 [{kv.Key}] 被以下语言使用: {string.Join(", ", names)}");
            }

            return groups;
        }

        // ================================================================
        //  字符收集（按字体维度）
        // ================================================================

        /// <summary>
        /// 为某个字体所覆盖的所有语言，收集其多语言文本中出现的全部唯一字符。
        /// 此方法只做纯数据提取，不涉及任何资源加载或生成逻辑，确保低耦合。
        /// </summary>
        /// <param name="langs">使用该字体的语言配置列表</param>
        /// <param name="contentTable">多语言内容表</param>
        /// <param name="settingKeys">设置面板 key 集合</param>
        /// <param name="settingOnly">
        ///   false = 收集普通文本字符（排除设置 key）；
        ///   true  = 仅收集设置面板文本字符。
        /// </param>
        /// <returns>去重后的字符集合</returns>
        private static HashSet<char> CollectCharsForFontGroup(
            List<ConfLanguage> langs,
            TbLanguageContent contentTable,
            HashSet<string> settingKeys,
            bool settingOnly)
        {
            HashSet<char> charSet = new HashSet<char>();

            // 遍历所有多语言内容条目
            foreach (var contentEntry in contentTable.DataMap)
            {
                bool isSettingKey = settingKeys.Contains(contentEntry.Key);

                // 按 settingOnly 过滤
                if (settingOnly && !isSettingKey) continue;
                if (!settingOnly && isSettingKey) continue;

                string[] values = contentEntry.Value.Value;

                // 遍历使用该字体的每种语言，按 LanguageCode 取对应索引的文本
                foreach (var langCfg in langs)
                {
                    int idx = (int)langCfg.LanguageCode - 1;
                    if (idx < 0 || idx >= values.Length) continue;

                    string text = values[idx];
                    if (string.IsNullOrEmpty(text)) continue;

                    foreach (char c in text)
                        charSet.Add(c);
                }
            }
            //添加白名单
            foreach (var writeChar in GenerateTMPDefine.WhiteList)
            {
                charSet.Add(writeChar);
            }
            return charSet;
        }
        
        /// <summary>
        /// 按顺序排序
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        private static string GetAllCharsByString(HashSet<char> chars)
        {
            var str = string.Concat(chars.OrderBy(c => (int)c));
            return str;
        }

        /// <summary>
        /// 构建「设置面板 key」集合。
        /// TbLanguage 中的每条记录的 Name 即为语言选择的 key，
        /// 这些 key 对应的文本属于设置面板专属文本，
        /// 需要单独导出为 _Setting 字体。
        /// </summary>
        private static HashSet<string> BuildSettingKeySet(TbLanguage langTable)
        {
            var set = new HashSet<string>();
            foreach (var kv in langTable.DataMap)
            {
                if (!string.IsNullOrEmpty(kv.Value.Name))
                    set.Add(kv.Value.Name);
            }
            return set;
        }

        // ================================================================
        //  字体 SDF 资源生成
        // ================================================================

        /// <summary>
        /// 加载一款源字体（.ttf），创建对应的 TMPro SDF FontAsset 并保存为 .asset 文件。
        /// 先校验并移除字体中不存在的字符，再以 256×256 起步的图集预烘焙项目相关字符，
        /// 若图集装不下则逐步扩容重试（256→512→1024→...→4096）。
        /// 烘焙阶段使用Dynamic；正式产物为Static，缺字或容量不足必须失败。
        /// 
        /// 生成流程参照 TMPro 标准做法：
        ///   - 新建时：创建 Material → UpdateMaterialProperty → AddObjectToAsset 嵌入子资源
        ///   - 更新时：通过 CopySerialized 保留旧的材质/图集引用
        ///   - 收尾：SetTextureReadable → ReadFontAssetDefinition → ImportAsset →
        ///           ON_FONT_PROPERTY_CHANGED
        /// 
        /// 注意：一种 .ttf 只生成一份 SDF 资源。
        /// 若多种语言共用同一字体，它们共享这份资源。
        /// 语言级别的差异（如 BaseLine）应在 UI 组件层面处理，而非字体资源层面。
        /// </summary>
        /// <param name="fontFileName">字体文件名（如 "AaFengKuangYuanShiRen.ttf"）</param>
        /// <param name="characters">该字体需要支持的全部唯一字符（会被原地修改：移除字体不存在的字符）</param>
        /// <param name="outDir">输出目录</param>
        /// <param name="nameSuffix">资源名后缀（"" 或 "_Setting"）</param>
        private static void BuildFontAsset(string fontFileName, HashSet<char> characters,
            string outDir, string nameSuffix)
        {
            if (characters == null || characters.Count == 0) return;

            // -- 加载源字体（配置表可能不带扩展名，自动补 .ttf） --
            string fontFile = fontFileName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
                ? fontFileName : fontFileName + ".ttf";
            string srcPath = Path.Combine(SourceFontDir, fontFile);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(srcPath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException($"源字体缺失: {srcPath}");
            }

            // -- 计算资源名与路径 --
            string fontBaseName = Path.GetFileNameWithoutExtension(fontFileName);
            string assetName    = $"{fontBaseName}{nameSuffix} SDF.asset";
            string assetPath    = Path.Combine(outDir, assetName).Replace('\\', '/');
            string assetCoreName = Path.GetFileNameWithoutExtension(assetName);

            // -- 校验并移除字体中不存在的字符（避免 TryAddCharacters 因无字形字符无限扩容） --
            characters.RemoveWhere(char.IsControl);
            int removedCount = RemoveMissingGlyphs(sourceFont, characters, fontBaseName, nameSuffix);
            if (removedCount > 0)
            {
                throw new InvalidOperationException($"字体 {fontBaseName}{nameSuffix} 缺少 {removedCount} 个必需字符，见字形校验日志；禁止导出不完整静态图集。");
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning($"[字体生成] '{fontBaseName}{nameSuffix}' 移除缺失字形后无可用字符，跳过生成。");
                return;
            }

            // -- 将字符集转为字符串（按 Unicode 排序，确保 TryAddCharacters 顺序稳定） --
            string charString = GetAllCharsByString(characters);

            // -- 创建字体，从 256×256 起步逐步扩容，最大 4096 --
            const int startAtlasSize = 256;
            if (!TryGenerateFontAsset(out TMP_FontAsset fontAsset, sourceFont,
                    charString, startAtlasSize, startAtlasSize,
                    AtlasPadding, SamplingPointSize, fontBaseName, nameSuffix))
            {
                Debug.LogError($"[字体生成] 创建 FontAsset 失败: {assetName}");
                return;
            }

            // ================================================================
            // 保存/更新资源，保留已有GUID与材质引用
            // ================================================================

            // 设置名字（必须在 CopySerialized 之前）
            fontAsset.name = assetCoreName;
            fontAsset.atlasTexture.name = $"{assetCoreName} Atlas";

            // 获取已有的字体资源
            TMP_FontAsset oldFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

            if (oldFontAsset != null)
            {
                // ---- 更新已有资源：通过 CopySerialized 增量更新，保留旧材质引用 ----
                var material     = oldFontAsset.material;
                var atlasTexture = oldFontAsset.atlasTexture;
                List<TMP_FontAsset> fullBacks = new List<TMP_FontAsset>();
                fullBacks.AddRange(oldFontAsset.fallbackFontAssetTable);
                EditorUtility.CopySerialized(fontAsset, oldFontAsset);
                EditorUtility.CopySerialized(fontAsset.atlasTexture, atlasTexture);
                oldFontAsset.material = material;
                oldFontAsset.atlasTextures[0] = atlasTexture;

                // 更新材质属性
                GenerateTMPUtility.UpdateMaterialProperty(material, oldFontAsset);
                //关联fullback
                foreach (var tmpFontAsset in fullBacks)
                {
                    if (tmpFontAsset != null && tmpFontAsset != oldFontAsset && !oldFontAsset.fallbackFontAssetTable.Contains(tmpFontAsset))
                        oldFontAsset.fallbackFontAssetTable.Add(tmpFontAsset);
                }
                fontAsset = oldFontAsset;
            }
            else
            {
                // ---- 新建资源：创建材质并绑定，子资源嵌入主 .asset ----
                Material material = new Material(Shader.Find(SDFShaderName));
                material.name = $"{assetCoreName} Material";
                GenerateTMPUtility.UpdateMaterialProperty(material, fontAsset);
                fontAsset.material = material;

                AssetDatabase.CreateAsset(fontAsset, assetPath);
                AssetDatabase.AddObjectToAsset(material, fontAsset);
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }

            // ---- 后处理 ----
            // 静态设置
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
            // 清空源字体映射
            SerializedObject serializedFontAsset = new SerializedObject(fontAsset);
            SerializedProperty sourceFontProp = serializedFontAsset.FindProperty("m_SourceFontFile_EditorRef");
            if (sourceFontProp != null)
            {
                sourceFontProp.objectReferenceValue = null;
                serializedFontAsset.ApplyModifiedProperties();
            }
            
            GenerateTMPUtility.SetTextureReadable(fontAsset.atlasTexture, false);
            // 设置图集纹理为不可读（优化内存/性能）
            GenerateTMPUtility.SetTextureReadable(fontAsset.atlasTexture, false);
            // 重新读取字体定义，确保内部数据一致
            fontAsset.ReadFontAssetDefinition();
            // 保存
            AssetDatabase.SaveAssets();
            // 重新导入字体资源以刷新 Unity 内部缓存
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset));
            // 通知 TMPro 字体属性已变更（刷新使用该字体的 Text 组件）
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
            Debug.Log($"[字体生成] ✔ {assetPath}  |  字符数={characters.Count}");
        }

        /// <summary>
        /// 尝试创建 SDF 字体资源并预烘焙指定字符。
        /// 从给定的 atlasWidth/Height 开始，若字符无法全部装入图集，
        /// 则逐步扩容（交替扩宽/扩高，256×256→512×256→512×512→...→4096×4096）。
        /// 最终仍失败则回退到 Dynamic 模式（运行时按需生成）。
        /// </summary>
        /// <returns>true = 成功创建 FontAsset（至少尝试了添加字符）</returns>
        private static bool TryGenerateFontAsset(out TMP_FontAsset fontAsset, Font sourceFont,
            string charString, int atlasWidth, int atlasHeight,
            int padding, int samplingPointSize,
            string fontBaseName, string nameSuffix)
        {
            fontAsset = null;
            int currentW = atlasWidth;
            int currentH = atlasHeight;

            while (true)
            {
                // 如果之前创建的失败资源还在，需要销毁
                if (fontAsset != null)
                {
                    UnityEngine.Object.DestroyImmediate(fontAsset);
                    fontAsset = null;
                }

                // 创建 TMP_FontAsset（Dynamic 模式兜底）
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    samplingPointSize,
                    padding,
                    RenderMode,
                    currentW,
                    currentH,
                    AtlasPopulationMode.Dynamic,
                    false
                );

                // 创建失败，直接返回
                if (fontAsset == null)
                {
                    Debug.LogError($"[字体生成] TMP_FontAsset.CreateFontAsset 返回 null，" +
                                   $"图集={currentW}×{currentH}");
                    return false;
                }

                // 尝试添加项目相关字符
                if (fontAsset.TryAddCharacters(charString, out var missingChars))
                {
                    // 全部字符装入成功
                    Debug.Log($"[字体生成] '{fontBaseName}{nameSuffix}' 字符预烘焙成功，" +
                               $"图集={currentW}×{currentH}，字符数={charString.Length}");
                    return true;
                }

                // 图集大小不足，逐步扩容（交替扩宽/扩高，256×256→512×256→512×512→...→4096×4096）
                if (currentW >= AtlasMaxSize && currentH >= AtlasMaxSize)
                {
                    // 已达到最大图集尺寸 4096×4096，无法继续扩容
                    break;
                }

                if (currentW <= currentH)
                {
                    // 宽度 ≤ 高度：扩宽
                    int newW = Math.Min(currentW * 2, AtlasMaxSize);
                    Debug.LogWarning($"[字体生成] '{fontBaseName}{nameSuffix}' 图集宽度不足，" +
                                     $"从 {currentW}×{currentH} 扩宽至 {newW}×{currentH} 重试 ...");
                    currentW = newW;
                }
                else
                {
                    // 高度 < 宽度：扩高
                    int newH = Math.Min(currentH * 2, AtlasMaxSize);
                    Debug.LogWarning($"[字体生成] '{fontBaseName}{nameSuffix}' 图集高度不足，" +
                                     $"从 {currentW}×{currentH} 扩高至 {currentW}×{newH} 重试 ...");
                    currentH = newH;
                }
            }

            // 最终产物为Static，不能再声称运行时Dynamic兜底。
            if (fontAsset != null) UnityEngine.Object.DestroyImmediate(fontAsset);
            fontAsset = null;
            throw new InvalidOperationException($"字体 {fontBaseName}{nameSuffix} 超过最大图集容量，导出已停止。");
        }

        /// <summary>
        /// 通过 FontEngine 检测源字体中缺失字形的字符，并从字符集中移除。
        /// 避免 TryAddCharacters 尝试添加无字形字符导致不必要的图集扩容。
        /// </summary>
        /// <returns>被移除的字符数量</returns>
        private static int RemoveMissingGlyphs(Font sourceFont, HashSet<char> characters,
            string fontBaseName, string suffix)
        {
            FontEngineError err = FontEngine.LoadFontFace(sourceFont, SamplingPointSize);
            if (err != FontEngineError.Success)
            {
                Debug.LogWarning($"[字形校验] 无法加载字体 '{fontBaseName}{suffix}' 到 FontEngine ({err})，跳过校验。");
                return 0;
            }

            var missing = new List<char>();
            foreach (char c in characters)
            {
                if (!FontEngine.TryGetGlyphIndex((uint)c, out _))
                    missing.Add(c);
            }

            if (missing.Count == 0) return 0;

            // 从字符集中移除字体不存在的字符
            foreach (char c in missing)
                characters.Remove(c);

            // 日志：输出前 N 个缺失字符示例
            int maxShow = 30;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[字形校验] ⚠ '{fontBaseName}{suffix}' 字体缺少 {missing.Count} 个字形，已移除:");

            int showCount = Math.Min(missing.Count, maxShow);
            for (int i = 0; i < showCount; i++)
            {
                sb.Append($"  U+{(int)missing[i]:X4}({missing[i]})");
                if ((i + 1) % 5 == 0 || i == showCount - 1) sb.AppendLine();
            }

            if (missing.Count > maxShow)
                sb.AppendLine($"  ... 及其他 {missing.Count - maxShow} 个字符");

            Debug.LogWarning(sb.ToString());
            return missing.Count;
        }

        // ================================================================
        //  工具方法
        // ================================================================

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void ShowError(string msg)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"[字体生成] {msg}");
            EditorUtility.DisplayDialog("字体生成", msg, "确定");
        }

    }
}