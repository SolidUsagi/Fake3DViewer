# Fake3DViewer
RGBイメージ＋デプスマップの画像・動画をLooking Glassで立体表示するUnityアプリケーションです。  

<img src="https://github.com/SolidUsagi/Fake3DViewer/blob/master/Assets/StreamingAssets/Images/Fuyu1_180LR.jpg" width="512">

## 動作環境
- Windows
- [The Looking Glass](https://lookingglassfactory.com/)

## 使い方
RGBイメージ＋デプスマップの画像ファイルや動画ファイルをドラッグ＆ドロップするとLooking Glassに表示されます。  
ドロップされた画像・動画は再生リストに登録され、再生リスト内の画像・動画はLooking Glassの左右ボタンまたはカーソルキー左右で順番に切り替えて表示することができます。  
〇ボタンまたはSpaceキーでスライドショーを開始します。  

再生リストは以下のXMLファイルに保存されます。  
C:\\Users\\<user_name>\\AppData\\LocalLow\\SolidUsagi\\Fake3DViewer\\Defaults.xml  
画像・動画が表示されているときにDeleteキーを押すと、再生リストから取り除かれます。  

なお、Unityエディター上で動作しているときはドラッグ＆ドロップができません。  

## 画像形式
通常の矩形イメージの他、Equirectangular形式に対応しています。ファイルをドロップしたらF2キーを押して適切な形式に切り替えてください。  
\[ F2 \]: 矩形 → Equirectangular\(180度\) → Equirectangular\(360度\) → 矩形 →・・・  

RGBイメージとデプスマップの並び方向はF3キー、並び順はF4キーで切り替えることができます。  
\[ F3 \]: SBS\(横並び\) → TAB\(縦並び\) → SBS →・・・  
\[ F4 \]: 押すたびにRGBイメージとデプスマップが入れ替わります  

デプスマップの深度の向きはF1キーで切り替えることができます。  
\[ F1 \]: 白手前 → 黒手前 → 白手前 →・・・  

画像形式の情報は再生リストに記録されます。  

## 複数ファイル
RGBイメージとデプスマップがそれぞれ別のファイルになっている画像\(動画は不可\)は、両方をまとめてドロップしてください。  
このとき、どちらかのファイル名に「depth」という文字列が含まれていれば、それをデプスマップファイルと見なします。どちらにも含まれていない場合はドロップした後にF4キーを押して正しく表示されるように切り替えてください。  

RGBイメージファイルとデプスマップファイルが同一フォルダ内にあり、デプスマップファイル名がRGBイメージファイル名に接頭辞「depth_」または接尾辞「_depth」を付加したものである場合は、どちらか片方のファイルをドロップするだけで表示されます。  
例えば、RGBイメージファイル名が「sample_image」ならば、デプスマップファイル名は「depth_sample_image」または「sample_image_depth」にしてください。  

## マウス操作
表示中の画像・動画をドラッグして動かすことができます。マウスホイールで拡大・縮小もできます。  
ダブルクリックすると初期位置・初期倍率に戻ります。  

## スクリーン調整
Holoplay Capture内に配置された、画像・動画を表示するスクリーンの位置とサイズ、奥行の長さを調整することができます。  
これらの調整値は再生リストに記録されます。調整値は画像・動画ごとに個別です。  

以下のキーを押しながらマウスホイールを回してください。それぞれのキーを押したままダブルクリックすると初期値に戻ります。  
\[ Z \]: スクリーンの位置\(前後方向\)  
\[ S \]: スクリーンのサイズ  
\[ D \]: 奥行の長さ\(立体感の調整\)  

## ライセンス
このソフトウェアは、MITライセンスのもとで公開されています。  
https://github.com/SolidUsagi/Fake3DViewer/blob/master/LICENSE  

## [The HoloPlay Unity SDK](https://docs.lookingglassfactory.com/Unity/)
Copyright 2017-18 Looking Glass Factory Inc. All rights reserved.  
https://github.com/SolidUsagi/Fake3DViewer/blob/master/Assets/Holoplay/License.pdf  

## [UnityWindowsFileDrag&Drop](https://github.com/Bunny83/UnityWindowsFileDrag-Drop)
Copyright (c) 2018 Markus Göbel (Bunny83)  
https://github.com/Bunny83/UnityWindowsFileDrag-Drop/blob/master/LICENSE  

## [PanoramaVideoWithUnity](https://github.com/makoto-unity/PanoramaVideoWithUnity)
Copyright (c) 2015 Makoto Ito  
https://github.com/makoto-unity/PanoramaVideoWithUnity/blob/master/LICENSE  
Sphere100.fbx を使用させていただきました。  
