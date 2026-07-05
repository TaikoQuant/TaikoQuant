# OpenTaiko → TaikoQuant rename map

- **CStage演奏画面共通** → **StagePlayScreenCommon**
- **CStage演奏ドラム画面** → **StageDrumPlayScreen**
- **CStage演奏ドラム画面** (file name) → **StageDrumPlayScreen.cs**
- **CActImplGauge** → **ActGaugeImpl**
- **CActImplRoll** → **ActRollImpl**
- **CActImplMtaiko** → **ActMtaikoImpl**
- **CActImplBackground** → **ActBackgroundImpl**
- **CAct演奏Drumsゲームモード** → **ActPlayDrumsGameMode**
- **CAct演奏Drumsゲームモード** (file name) → **ActPlayDrumsGameMode.cs**
- **CAct演奏演奏情報** → **ActPlayInfo**
- **CAct演奏パネル文字列** → **ActPanelString**
- **CAct演奏スコア共通** → **ActScoreCommon**
- **CAct演奏Combo共通** → **ActComboCommon**
- **CAct演奏AVI** → **ActAvi**
- **CActTaikoScrollSpeed** → **ActTaikoScrollSpeed**
- **CStage結果** → **StageResult**
- **CStage演奏画面共通** (namespace) → **TaikoQuant.Core.Scenes** (kept, only class renamed)
- **CAct** prefix generally replaced with **Act** and **CStage** with **Stage**

*Notes*: All occurrences of the `OpenTaiko.` prefix should be removed; the remaining class names are renamed according to the map above.
