using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectoryConnect
{
    public class DirectoryOperate
    {
        /// <summary>
        /// 処理中にメッセージを表示するメソッド
        /// </summary>
        /// <param name="strDispMessage">表示するメッセージ</param>
        public delegate void DispMessage(
            string strDispMessage
        );

        /// <summary>
        /// コピーの全体的な進捗と個別の進捗を表示するメソッド
        /// </summary>
        /// <param name="strDispSrcDir">現在のコピー中ディレクトリ</param>
        /// <param name="strDispDestDir">現在のコピー先ディレクトリ</param>
        /// <param name="lProg">現在コピー中のディレクトリの進捗</param>
        /// <param name="lFileCount">現在コピー中のディレクトリのファイル数</param>
        /// <param name="lAllProg">全コピー対象ディレクトリを通した進捗</param>
        /// <param name="lAllFileCount">全コピー対象ディレクトリのファイル総数</param>
        public delegate void DispOverallProgress(
            string strDispSrcDir,
            string strDispDestDir,
            long lProg,
            long lFileCount,
            long lAllProg, 
            long lAllFileCount
        );

        /// <summary>
        /// コピー元ディレクトリ、コピー先ディレクトリ、コピー元ディレクトリのファイル数を整理する
        /// </summary>
        private class CopyDirInfo
        {
            public string strSrc = ""; //コピー元ディレクトリ
            public string strDest = ""; //コピー先ディレクトリ
            public long strSrcFileCount = -1; //コピー元ディレクトリのファイル数
        }

        /// <summary>
        /// インスタンス化を行います。
        /// </summary>
        public DirectoryOperate()
        {
        }

        /// <summary>
        /// 指定したディレクトリにあるファイルやサブディレクトリをコピーします。
        /// </summary>
        /// <param name="strMessage">処理の進捗状況やエラーメッセージを格納</param>
        /// <param name="strSrcAndDest">コピー元ディレクトリとコピー先ディレクトリの組み合わせを|で連結した文字列</param>
        /// <param name="delDispAllProg">
        /// 第一引数：string型、第二引数：string型、第三引数：long型、第四引数：long型、第五引数：long型、第六引数：long型、戻り値なしの進捗を表示するメソッドを指定します。
        /// 第一引数には現在コピー中のディレクトリが渡されます。
        /// 第二引数には現在のコピー先ディレクトリが渡されます。
        /// 第三引数には何件コピーが終わったかが渡されます。
        /// 第四引数にはコピー中のディレクトリのファイル数が渡されます。
        /// 第五引数にはすべてのコピー対象ディレクトリを通して何件コピーが終わったかが渡されます。
        /// 第六引数にはすべてのコピー対象ディレクトリにあるファイル総計が渡されます。
        /// </param>
        /// <param name="delDispMsg">
        /// 第一引数：string型、戻り値なしの進捗を表示するメソッドを指定します。
        /// 第一引数には進捗に関するメッセージが渡されます。
        /// </param>
        /// <returns>
        /// 0：エラーなく処理完了。
        /// -1：ファイルのコピーまたはサブディレクトリ作成処理時にエラーが発生。
        /// -2：ファイルコピーまたはサブディレクトリ作成処理以外でエラーが発生。
        /// </returns>
        public int BulkCopyDirectory(
                ref string strMessage,
                List<string> strSrcAndDest,
                DispOverallProgress delDispAllProg,
                DispMessage delDispMsg
            )
        {
            long lAllFileCount = 0;
            List<CopyDirInfo> copyDirInfo = new List<CopyDirInfo>();
            Boolean isFailure = false; //ファイル数の集計に失敗したディレクトリはあったか
            int iResultMain = 0; //このメソッドの戻り値
            int iResultSub = 0;  //SupportBulkCopyDirectoryメソッドの戻り値
            long lAllProg = 0;   //すべてのコピー対象ディレクトリに対しての進捗

            //全てのコピー元ディレクトリにあるファイル総数を取得する
            //コピー処理に使う値を整理する
            for (int i1 = 0; i1 < strSrcAndDest.Count; i1++)
            {
                int iResult = 0;
                long lFileCount = 0;
                string[] strEles = strSrcAndDest[i1].Split('|');

                //対象ディレクトリのファイル総数を求める
                iResult = CountFile(ref lFileCount, strEles[0]);
                if(iResult == -1)
                {
                    isFailure = true;
                } else if (iResult == -2)
                {
                    return -2;
                }
                lAllFileCount += lFileCount;

                //コピーに使う値を整理する
                CopyDirInfo wkCopyDirInfo = new CopyDirInfo();
                wkCopyDirInfo.strSrc = strEles[0];
                wkCopyDirInfo.strDest = strEles[1];
                wkCopyDirInfo.strSrcFileCount = lFileCount;
                copyDirInfo.Add(wkCopyDirInfo);
            }

            //進捗に誤差が出る可能性があることを表示する
            if(isFailure)
            {
                delDispMsg("コピー対象フォルダーにあるファイル総数を正確に集計できませんでした。" + Environment.NewLine +
                    "進捗の表記に誤差が生じる可能性があります。" + Environment.NewLine +
                     Environment.NewLine);
            }

            //ディレクトリにあるファイルやサブディレクトリをコピーする
            for (int i1 = 0; i1 < copyDirInfo.Count; i1++)
            {
                string strErrMsgSub = "";   //処理の進捗状況やエラーメッセージの取得用
                long lProg = 0;             //個別の進捗状況
                
                //ファイルやディレクトリのコピーを行う
                iResultSub = SupportBulkCopyDirectory(
                                ref strErrMsgSub,
                                copyDirInfo[i1].strSrc,
                                copyDirInfo[i1].strDest,
                                delDispAllProg,
                                copyDirInfo[i1].strSrc,
                                copyDirInfo[i1].strDest,
                                ref lProg,
                                copyDirInfo[i1].strSrcFileCount,
                                ref lAllProg,
                                lAllFileCount
                            );
                if(iResultSub == 0) {
                    strMessage +=
                       "■ " + copyDirInfo[i1].strSrc + " のデータを " + copyDirInfo[i1].strDest + " へのコピーに成功しました。" + Environment.NewLine +
                       Environment.NewLine;
                }
                else if (iResultSub == -1){
                    strMessage +=
                        "■ " + copyDirInfo[i1].strSrc + " のデータを " + copyDirInfo[i1].strDest + " へのコピーする途中で一部のファイルのコピーが失敗しました。" + Environment.NewLine +
                        Environment.NewLine +
                        strErrMsgSub +
                        Environment.NewLine;
                    iResultMain = -1;
                }
                else if (iResultSub == -2){
                    strMessage +=
                        "■ " + copyDirInfo[i1].strSrc + " のデータを " + copyDirInfo[i1].strDest + " へのコピーする途中で問題が発生したためバックアップを中止しました。" + Environment.NewLine +
                        Environment.NewLine +
                        strErrMsgSub +
                        Environment.NewLine;
                    return -2;
                }
                else
                    { }
            }
            return iResultMain;
        }

        /// <summary>
        /// 引数で指定したディレクトリのファイル総数を取得します。
        /// </summary>
        /// <param name="lFileCount">ファイル総数</param>
        /// <param name="targetDir">対象ディレクトリ</param>
        /// <returns>
        /// 集計処理でエラーが発生した際は-1を返す
        /// 集計処理以外でエラーが発生した場合は-2を返す
        /// </returns>
        private int CountFile(
                ref long lFileCount,
                string targetDir
            )
        {
            try
            {
                string[] strTargetFiles;                                                    //対象ディレクトリのファイル一覧
                string strRecycleDir = targetDir[0] + @":\$RECYCLE.BIN";                    //対象ディレクトリのドライブのRECYCLE.BINディレクトリ
                string strSysVolInfoDir = targetDir[0] + @":\System Volume Information";    //対象ディレクトリのドライブのドライブのSystem Volume Informationディレクトリ
                string[] strSubDirs;                                                        //対象ディレクトリのサブディレクトリ一覧
                int iResultMainDir = 0;                                                     //対象ディレクリの集計結果
                int iResultSubDir = 0;　                                                    //対象サブディレクリの集計結果

                //対象ディレクトリ末尾に"\"をつける
                if (targetDir[targetDir.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    targetDir = targetDir + System.IO.Path.DirectorySeparatorChar;
                }

                //対象ディレクトリのファイル数を累計値に加算する
                try
                {
                    strTargetFiles = System.IO.Directory.GetFiles(targetDir);
                    lFileCount += strTargetFiles.Length;
                }
                catch (Exception)
                {
                    return -1;
                }

                //サブディレクトリのファイル数をカウントする
                // サブディレクトリ一覧を取得する
                strSubDirs = System.IO.Directory.GetDirectories(targetDir);

                // 各サブディレクトリに対して集計を行う
                foreach (string strSubDir in strSubDirs)
                {
                    // RECYCLE.BINディレクトリ と System Volume Informationディレクトリは処理の対象にしない
                    if (strSubDir == strRecycleDir || strSubDir == strSysVolInfoDir)
                    {
                        continue;
                    }

                    iResultSubDir = CountFile(ref lFileCount, strSubDir);

                    if (iResultSubDir == -1)
                    {
                        iResultMainDir = -1;
                    }
                    if (iResultSubDir == -2)
                    {
                        return -2;
                    }
                }
                return iResultMainDir;
            }
            catch (Exception)
            {
                return -2;
            }
        }

        /// <summary>
        /// 再起処理を用いてファイルやサブディレクトリのコピーを行います。
        /// </summary>
        /// <param name="strErrMsg">エラーメッセージ</param>
        /// <param name="strSrcDir">コピー元ディレクトリ</param>
        /// <param name="strDestDir">コピー先ディレクトリ</param>
        /// <param name="delDispAllProg">コピーの全体的な進捗と個別の進捗を表示するメソッド</param>
        /// <param name="strDispSrcDir">進捗表示用の現在コピー中のディレクトリ</param>
        /// <param name="strDispDestDir">進捗表示用の現在コピー先ディレクトリ</param>
        /// <param name="lProg">現在コピー中のディレクトリの進捗</param>
        /// <param name="lFileCount">現在コピー中のディレクトリのファイル数</param>
        /// <param name="lAllProg">すべてのコピー対象ディレクトリを通した進捗</param>
        /// <param name="lAllFileCount">すべてのコピー対象ディレクトリのファイル数</param>
        /// <returns>
        /// 0：エラーなく処理完了、
        /// -1：ファイルのコピーまたはサブディレクトリ作成処理時にエラーが発生、
        /// -2：ファイルコピーまたはサブディレクトリ作成処理以外でエラーが発生
        /// </returns>
        private int SupportBulkCopyDirectory(
                ref string strErrMsg,
                string strSrcDir,
                string strDestDir,
                DispOverallProgress delDispAllProg,
                string strDispSrcDir,
                string strDispDestDir,
                ref long lProg,
                long lFileCount,
                ref long lAllProg,
                long lAllFileCount
            )
        {
            try
            {
                string strSrcDirSepa = "";      //コピー元ディレクトリ末尾に"\"有り
                string destDirSepa = "";        //コピー先ディレクトリ末尾に"\"有り
                string[] strSrcfiles;           //コピー元ディレクトリにあるファイル一覧
                int iResultMainDir = 0;         //コピー元ディレクリに対するコピー結果
                int iResultSubDir = 0;          //コピー元サブディレクトリに対するコピー結果
                string strDestSubDirSepa = "";  //コピー先サブディレクトリ末尾に"\"有り
                string[] strSubDirs;            //コピー元ディレクトリ一覧
                string strRecycleDir = "";      //コピー元ドライブのRECYCLE.BINディレクトリ
                string strSysVolInfoDir = "";   //コピー元ドライブのSystem Volume Informationディレクトリ

                //コピー元のディレクトリの末尾の"\"をつけた形式を取得
                if (strSrcDir[strSrcDir.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                { strSrcDirSepa = strSrcDir + System.IO.Path.DirectorySeparatorChar; }
                else
                { strSrcDirSepa = strSrcDir; }

                //コピー先のディレクトリ名の末尾に"\"を付ける
                if (strDestDir[strDestDir.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                { destDirSepa = strDestDir + System.IO.Path.DirectorySeparatorChar; }
                else
                { destDirSepa = strDestDir; }

                //（存在しない場合は）コピー先ディレクトリを作る
                if (!System.IO.Directory.Exists(destDirSepa))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(destDirSepa);

                        //ドライブ直下の場合を除き属性を付与する
                        if(strSrcDirSepa.Length > 3)
                        { System.IO.File.SetAttributes(destDirSepa, System.IO.File.GetAttributes(strSrcDirSepa)); }
                    }
                    catch (Exception e)
                    {
                        strErrMsg += destDirSepa + " を作るときにエラーが発生しました。" + Environment.NewLine +
                                       e.Message + Environment.NewLine + Environment.NewLine;

                        //ディレクトリを作れなかった時点で呼び出し元へ処理を戻す
                        return -1;
                    }
                }

                //コピー元ディレクトリのファイルをコピー先ディレクトリへコピーする
                try
                {
                    strSrcfiles = System.IO.Directory.GetFiles(strSrcDirSepa);
                }
                catch (Exception e)
                {
                    strErrMsg += strSrcDirSepa + " のコピー中にエラーが発生しました。" + Environment.NewLine +
                                   e.Message + Environment.NewLine + Environment.NewLine;

                    //エラーが発生した時点で呼び出し元へ処理を戻す
                    return -1;
                }

                foreach (string strSrcFile in strSrcfiles)
                {
                    try
                    {
                        System.IO.File.Copy(strSrcFile, destDirSepa + System.IO.Path.GetFileName(strSrcFile), false);

                        //コピーの全体的な進捗と個別の進捗を表示する
                        lProg += 1;
                        lAllProg += 1;
                        delDispAllProg(strDispSrcDir, strDispDestDir, lProg, lFileCount, lAllProg, lAllFileCount);
                    }
                    catch (Exception e)
                    {
                        strErrMsg += strSrcFile + " を " + destDirSepa + " へコピーするときにエラーが発生しました。" + Environment.NewLine +
                                       e.Message + Environment.NewLine + Environment.NewLine;
                        iResultMainDir = -1;
                    }
                }

                //コピー元サブディレクトリのファイルをコピーする
                strRecycleDir = strSrcDirSepa[0] + @":\$RECYCLE.BIN";
                strSysVolInfoDir = strSrcDirSepa[0] + @":\System Volume Information";

                // コピー元のサブディレクトリ一覧を取得する
                strSubDirs = System.IO.Directory.GetDirectories(strSrcDirSepa);

                foreach (string strSubDir in strSubDirs)
                {
                    //RECYCLE.BINディレクトリ と System Volume Informationディレクトリはコピーしない
                    if (strSubDir == strRecycleDir || strSubDir == strSysVolInfoDir)
                    {
                        continue;
                    }

                    //コピー元サブディレクトリのコピー先ディレクトリを決定する
                    // {現在のコピー先ディレクトリ}\{コピー元サブディレクトリの末端フォルダー名}\
                    strDestSubDirSepa = destDirSepa
                        + strSubDir.Substring
                          (strSubDir.LastIndexOf(@"\") + 1,
                            strSubDir.Length - strSubDir.LastIndexOf(@"\") - 1)
                              + System.IO.Path.DirectorySeparatorChar;

                    //コピー元サブディレクトリのファイルをコピーする
                    iResultSubDir = SupportBulkCopyDirectory(
                            ref strErrMsg,
                            strSubDir,
                            strDestSubDirSepa,
                            delDispAllProg,
                            strDispSrcDir,
                            strDispDestDir,
                            ref lProg,
                            lFileCount,
                            ref lAllProg,
                            lAllFileCount
                        );
                    switch (iResultSubDir)
                    {
                        case 0:
                            break;
                        case -1:
                            iResultMainDir = -1;
                            break;
                        case -2:
                            return -2;
                        default:
                            break;
                    }
                }
                return iResultMainDir;
            }
            catch (Exception e)
            {
                strErrMsg = e.Message;
                return -2;
            }
        }
    }
}