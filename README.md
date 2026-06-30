<div align="center">

# Team314-2D

</div>

---

## 概要
チーム制作

---

## 必要なツール
- [Git](https://git-scm.com/install/windows)
- [SourceTree](https://www.sourcetreeapp.com/) (別のツール使うならそっちを使ってください)
- Unity Hub

## 環境
- Unity 6000.3.14f1(バージョン)
- OS:Windows

---

## 環境構築
1. リポジトリをクローンする(保存先のパスは英数字にすること)
<img width="649" height="438" alt="スクリーンショット 2026-05-12 184036" src="https://github.com/user-attachments/assets/656e5c1d-c192-4ee6-8192-d0b77f1c9557" />

2. プルする


3. 追加∨のディスクから加えるでさっき保存先にしたファイルを選択し名前のところにカーソルを合わせプロジェクトを開く。




<img width="255" height="117" alt="image" src="https://github.com/user-attachments/assets/f39cda2e-e070-4c2c-a55e-912363a74eb8" />


~~3. `.gitconfig`にスマートマージの設定を追加する(下記)~~

~~### スマートマージの設定~~
~~```~~
[merge]  
    tool = unityyamlmerge
[mergetool "unityyamlmerge"]  
	trustExitCode = false  
	cmd = 'C:/Program Files/Unity/Hub/Editor/6000.(使うバージョン)/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
~~```~~

## 便利ツール
### シーン切り替え君  
 Tools → シーン切り替え君 を使うとワンクリックでシーンを切り替えられます。  

## ブランチ運用
- `master` → リリース用
- `develop` → 開発用(ここにプッシュする)
- `feature/xxx` → 機能開発

## 開発フロー
1. 前の作業のマージが終わっているかを確認する
2. SorceTreeで `develop`から `feature/作業内容` ブランチを作成する。(絶対)
  >作業する直前以外で作成したブランチを使う場合は以下の手順で行ってください
  >
  > developをプル（最新の状態にする）  
  > 前回使ったブランチをダブルクリックでチェックアウト  
  > 上部メニューの「リポジトリ」→「マージ」をクリック  
  > 「origin/develop」のバッジがついているコミットを選択してOK  
  > Unityを起動  
  > Unityはマージが完了してから起動してください  
>
3. 作成したブランチにdevelopをプルする。
4. Discordの作業報告チャンネルに作業するシーンやPrefabを投稿する。
5. 作業開始
6. 作業したファイルをコミット(ローカルコミット、リモートコミットは作成したブランチを選択、プルダウンの選択肢にない場合は入力してください)、gitでプルリクを送る(develop)  
   Gitでプルリクを送る時プッシュする先がdevelopになっているかを確認してください  
   　プルリクを送る時にコンフリクトが起きた場合はSourceTreeのターミナルで  
   　`git mergetool`  
   　を実行し解決した後、再度コミットしプルリクを送ってください  
7. マージされたらDiscordに報告する  

## 注意事項
- 同じシーン・Prefabを同時に編集しない
- 作業前に必ずDiscordで作業内容を報告する

## コミットメッセージ
　何をしたのかを書いて送る。  

## コーディングルール
- コメントは日本語で書いてOK
- `.editorconfig` でUTF-8 BOM設定済みのため文字化けしません

