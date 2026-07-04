# TJAChartCore - .tja譜面読み込み・再生用ライブラリ
.tjaを読み込み、再生するためのライブラリです。  
再生といいつつ、当面の間は読み込み機能(とちょっとしたノーツ判定関数)のみを置いておきます。  
TJAPlayer3で再生できるギミックにいくつか対応しています。(#SUDDEN, #JPOSSCROLLなど)

## 公開の意図
このライブラリは**TJA再生シミュレーター「TaikøNauts」で実際に使用している譜面再生システムの一部を切り出したもの**です。  
譜面再生に関する問題が確認された際に、ユーザー側でも検証ができることを目的としています。  
また、シミュレーターを作る際のライブラリにも活用できることを想定しています。  
ただし、**実験的な側面が大きいため、ライブラリの利便性のための更新は基本的に行わない予定です**。

## How to Use
意味があるかはわかりませんが一応簡易的なコードを置いておきます。
```C#
TjaChartParser parser = new TjaChartParser();

string songPath = "C:\hoge.tja";
LoadType loadType = LoadType.Normal;
Song songData = parser.GetSongDataFromTja(songPath, loadType);
```

## 内部構造
### 譜面読み込み関連
- `Song`クラス `(TJAChartCore/Charts/Models/Song.cs)`
  - 楽曲のヘッダ情報 `_header` (`SongHeader`クラス)
  - 楽曲のコース情報 `_course` (`SongCourse`クラス)
- `SongHeader`クラス `(TJAChartCore/Charts/Models/SongHeader.cs)`
- `SongCourse`クラス `(TJAChartCore/Charts/Models/SongCourse.cs)`
  - レベル `_level` (`int`)
  - 譜面内に分岐が存在するかのフラグ `_hasBranch` (`bool`)
  - 譜面内の風船打数情報 `_balloon/_balloon{BranchName}/` (`List<int>`)
  - 譜面内のノート情報をまとめたリスト `_chip/_chip{BranchName}` (`List<Chip>`)
  - 通常コマンド・譜面分岐コマンドをまとめたリスト `_commands/_branchCommands` (`List<Command>`)
- `Chip`クラス (`TJAChartCore/Charts/Models/Chip.cs`)
  - ノート時間 `_time`
  - HBSCroll用ノーツ時間 `_timeForHBScroll`
  - ノート種別 `_noteType`
  - ノート文字種別 `_seNoteType`
  - その他各種ノート情報(詳しくは`TJAChartCore/Charts/Models/Chip.cs`をご覧ください。)

### 判定関連
- `Judgement`クラス `TJAChartCore/Enso/Judgement/Judgement.cs`
  - `GetNearestChip(time, startIndex, chips, course, don)`関数
    - `time`ミリ秒を基準に、`startIndex`番目から最も近いノーツを検索します。
    - 返すノートの判定基準は以下の通りです。
      - 過去のノートに対して、「判定が`Miss`でない、現在時間から最も遠いノート(A)」を取得します。
      - 未来のノートに対して、「判定が`Miss`でない、現在時間から最も近いノート(B)」を取得します。
        - (A)が存在しており、(B)が存在しない場合、(A)を返します。
        - (A)が存在せず、(B)が存在していた場合、(B)を返します。
        - (A)も(B)も存在した場合、(A)を返します。
  - `GetJudgeFromTime(time, chip)` 関数
    - `time`ミリ秒を基準に、`chip`がどれだけ離れているかを`NoteJudge`で返します。

## 免責事項
- このライブラリに「描画機能」は搭載していません。
- このライブラリを使用した際に発生する問題に関する対応は行いません。予めご了承ください。
