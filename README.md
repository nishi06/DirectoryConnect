# DirectoryConnect
# DirectoryOperate クラス
 DirectoryOperate はディレクトリーの操作を行うクラスです。
# コンストラクター
## DirectoryOperate()
 インスタンスを初期化します。
# メソッド
## BulkCopyDirectory
### 概要
 コピー元ディレクトリとコピー先ディレクトリをまとめて指定し、一括でファイルやサブディレクトリをコピーします。
### 引数
 + strMessage：String型。参照渡しでコピー中に発生したエラーメッセージを格納します。
 + strSrcAndDest：List<string>型。「コピー元|コピー先」のようにコピー元とコピー先ディレクトリを|で連結した文字列をList形式で指定します。
 + delDispAllProg：第一引数：string型、第二引数：string型、第三引数：long型、第四引数：long型、第五引数：long型、第六引数：long型、戻り値なしのメソッドを指定します。
     + 第一引数には現在コピー中のディレクトリが渡されます。
     + 第二引数には現在コピー先のディレクトリが渡されます。
     + 第三引数には現在コピー中のディレクトリで何件コピーが終わったかが渡されます。
     + 第四引数には現在コピー中のディレクトリのファイル数が渡されます。
     + 第五引数にはすべてのコピー対象ディレクトリを通して何件コピーが終わったかが渡されます。
     + 第六引数にはすべてのコピー対象ディレクトリにあるファイル総計が渡されます。
 + delDispMsg：第一引数：string型、戻り値なしの進捗を表示するメソッドを指定します。
     + 第一引数にはコピー対象フォルダーにあるファイル総数を正確に集計できなかった場合に、進捗の表記に誤差が生じる可能性がある旨のメッセージが渡されます。
### 戻り値
 + 0：エラーなく処理完了
 + -1：ファイルのコピーまたはサブディレクトリ作成処理時にエラーが発生
 + -2：ファイルのコピーまたはサブディレクトリ作成処理以外でエラーが発生
# 使用例（コンソールアプリケーション）
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProjectConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryConnect.DirectoryOperate dirOpe = new DirectoryConnect.DirectoryOperate();
            string processMessage = "";

            List<string> sourceAndDestination = new List<string>();
            sourceAndDestination.Add(@"D:\Data1|D:\TEST1");
            sourceAndDestination.Add(@"D:\Data2|D:\TEST2");

            int iResult = 0;
            iResult = dirOpe.BulkCopyDirectory(ref processMessage, sourceAndDestination, ProgressDisp , MessageDisp);

            //コンソールにバックアップの結果を表示する
            if (iResult == 0)
            {
                Console.Clear();
                Console.WriteLine("バックアップ処理が問題なく完了しました。");
                Console.WriteLine("");
                Console.WriteLine(processMessage);
            }
            else if (iResult == -1)
            {
                Console.Clear();
                Console.WriteLine("一部ファイルのバックアップに失敗しました。");
                Console.WriteLine("");
                Console.WriteLine(processMessage);
            }
            else if (iResult == -2)
            {
                Console.Clear();
                Console.WriteLine("バックアップ処理中に問題が発生したため処理を中止しました。");
                Console.WriteLine("");
                Console.WriteLine(processMessage);
            }
            Console.WriteLine("コピー処理が終わりました。");
            Console.Read();
        }

        /// <summary>
        /// 現在コピー中のディレクトリに関する進捗 および コピー対象ディレクトリ全体を通したコピー進捗をコンソール画面に表示する
        /// </summary>
        /// <param name="dispSourceDirectory">現在コピー中のディレクトリ</param>
        /// <param name="dispDestinationDirectory">現在コピー先のディレクトリ</param>
        /// <param name="progress">現在コピー中のディレクトリの進捗</param>
        /// <param name="fileCount">現在コピー中のディレクトリのファイル数</param>
        /// <param name="overallProgress">すべてのコピー対象ディレクトリに対しての進捗</param>
        /// <param name="overallFileCount">すべてのコピー対象ディレクトリのファイル数</param>
        static void ProgressDisp(
            string dispSourceDirectory,
            string dispDestinationDirectory,
            long progress,
            long fileCount,
            long overallProgress,
            long overallFileCount)
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.Write(
                "現在 " + dispSourceDirectory + " を " + dispDestinationDirectory + " へコピーしています。" + Environment.NewLine +
                "進捗： " + progress.ToString() + " / " + fileCount.ToString() + Environment.NewLine +
                "全体の進捗： " + overallProgress.ToString() + " / " + overallFileCount.ToString()
                );
        }

        /// <summary>
        /// コンソール画面に処理中に発生したエラーなどのメッセージを表示する
        /// </summary>
        /// <param name="dispSourceDirectory">コンソール画面に表示するメッセージ</param>
        static void MessageDisp(
            string dispMessage
            )
        {
            Console.Write(dispMessage);
        }
    }
}
```
