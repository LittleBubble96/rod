# 算筹素材提取审查（2026-10-10）

工具：内置 image_gen，使用 imagegen 技能。参考原图：`Art/效果图/02_对局页.png`。用户确认无分层源文件。

状态：**未通过，不可接入正式 UI，也不代表一比一还原完成。**

- `dice-bodies-candidate-v1.png`：三种点数底板，移除了面符号/数字，保留侧面刻线。边缘存在黄绿白杂色和孤立像素。
- 第二次仅要求修复 alpha 边缘，工具输出仍有明显边缘污染，没有采纳。没有对原始效果图或正式皮肤做覆盖。

首轮提示词（内置工具 edit，原图为唯一设计依据）：

> Use case: precise-object-edit. Asset type: production Unity UI sprite extraction sheet. The supplied screenshot is the ONLY approved visual design, an edit target, NOT inspiration. Preserve its dice geometry, camera angle, corner radii, ivory top surface, soft green/amber side materials, teal engravings, lighting and shading exactly. Do not redesign or embellish. Extract the THREE existing dice bodies from the lower tray: the ivory one-point body, the pale mint two-point body, and the amber three-point body. Remove ONLY face symbols, Arabic point numbers, selection check badges and selection outlines from the top faces so they can be rendered as independent UI overlays; retain the one/two/three dark teal horizontal engraved bars on the front sides respectively. Do not change silhouette or proportions. Arrange the three isolated bodies in a single horizontal row at identical scale on a REAL transparent alpha background, with generous transparent gaps and equal cells. The entire image must be a landscape sheet, no captions, no labels, no grid, no surrounding screenshot, no tray, no decorative additions. Each body must reproduce the exact source appearance at increased resolution; no creative interpretation. Keep the matte ivory top completely blank, restore just the underlying original material where the symbols and numbers were removed. Output clean production sprites with transparent background, not a checkerboard painted into the image.

二轮提示词（以 v1 为 edit target）：

> Precise alpha-edge repair only. This three-dice sprite sheet is the edit target. Preserve every opaque interior pixel, shape, scale, spacing, blank ivory face, ivory/mint/amber body, and one/two/three dark teal engravings. Change ONLY the transparent margins and boundary: remove ALL stray yellow/green/white jagged fringe pixels and isolated speckles around the silhouettes, especially left side of ivory die, between dice, top margins, and around amber die. Produce clean smooth antialiased alpha edges with no bright halo and no color contamination. The background must be genuinely transparent alpha, not black, not checkerboard. Do not redraw or reinterpret the dice, change proportions, add drop shadows or add any text. Keep original sheet dimensions and object positions.
