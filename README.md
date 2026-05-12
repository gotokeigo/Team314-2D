<div align="center">

# Practice

</div>

---

## 概要
このプロジェクトの簡単な説明を書く

---

## 必要なツール
- [Git](https://git-scm.com/install/windows)
- [SourceTree](https://www.sourcetreeapp.com/)
- Unity Hub

## 環境
- Unity 6000.(バージョン)
- OS:Windows

---

## 環境構築
1. リポジトリをクローンする
2. Unity Hubでプロジェクトを開く
3. `.gitconfig`にスマートマージの設定を追加する(下記)

### スマートマージの設定
```
[merge]  
    tool = unityyamlmerge
[mergetool "unityyamlmerge"]  
	trustExitCode = false  
	cmd = 'C:/Program Files/Unity/Hub/Editor/6000.(使うバージョン)/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
```

## 便利ツール
### シーン切り替え君  
 Tools → シーン切り替え君 を使うとワンクリックでシーンを切り替えられます。  
 BuildProfiels→ SceneListで作成したシーンを追加しておく必要があります。  

## ブランチ運用
- `master` → リリース用
- `develop` → 開発用(ここにプッシュする)
- `feature/xxx` → 機能開発

## 開発フロー
1. 前の作業のマージが終わっているかを確認する
2. SorceTreeで `develop`から `feature/作業内容` ブランチを作成する。
3. 作成したブランチにdevelopをプルする。
4. Discordの作業報告チャンネルに作業するシーンやPrefabを投稿する。
5. 作業開始
6. 作業したファイルをコミット、プルリクを送る(develop)  
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

