# Ruka Desktop Assistant — Character Storyboard v0.2.1

## Purpose
るかを「デスクトップに置いた画像」ではなく、デスクトップに住んでいるキャラクターとして見せるための基本絵コンテ。

## Scene 01 — Idle
- 立ち位置: デスクトップ端
- 表情: 通常
- 動き: 小さく上下に浮遊
- 台詞: なし
- 遷移: ユーザー操作、音声、時刻、アクティビティ

## Scene 02 — User Grabs Ruka
- 1コマ目: マウスカーソルがるかの上に入る
- 2コマ目: 左クリック開始
- 3コマ目: 掴んだ位置を維持したままマウスへ追従
- 4コマ目: マウスを離した位置で停止
- 必須条件: ワープしない、クリック位置がずれない

## Scene 03 — Walk
- 1コマ目: 歩き出す
- 2コマ目: walk-1
- 3コマ目: walk-2
- 4コマ目: 目的地へ到着
- 移動: 緩やかな補間
- 表示: 画面端から飛び出さない

## Scene 04 — Talk
- 表情: 会話
- 動き: 軽い上下/拡縮
- 吹き出し: るかの上側
- 音声: TTS
- 終了: idleへ戻る

## Scene 05 — Sleep
- 表情: 眠そう
- 動き: 小さな呼吸アニメーション
- 音声: 停止
- 吹き出し: 非表示

## Scene 06 — Watching Game
- ゲーム起動を検知
- るかを画面端へ移動
- ゲーム別Scaleを適用
- 音声設定に従う
- ゲーム終了後、通常位置へ復帰

## Scene 07 — Pause
- 「るか、待って」または一時停止
- 自律移動: 停止
- 音声入力: 停止
- 現在位置を維持
- 再開時: idle

## Asset plan
- Assets/ruka-idle.png
- Assets/ruka-talk.png
- Assets/ruka-sleep.png
- Assets/ruka-walk-1.png
- Assets/ruka-walk-2.png

## v0.2.1 implementation focus
1. ドラッグ時の座標ジャンプを修正
2. 掴んだ位置を維持して追従
3. ドラッグ終了時に位置を保存
4. 実機で表示・移動を確認
