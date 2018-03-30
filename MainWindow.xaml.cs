using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Linq;
using ComputeHashSelectFileLib;

namespace ComputeHashSelectFileWpfFram3_0
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    private bool intRptFlg = false;
    private delegate void DlgSetTxtBoxTxt(string txt);
    private DlgSetTxtBoxTxt _dlgSetTxtBoxTxt;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void setTxtBoxTxt(string txt)
    {
      this.textBox.Text = txt;
      procWitEnd();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      checkBoxSvnExl.IsChecked = true;
      this.LblWaitMess.Visibility = Visibility.Hidden;
      this.rctWait.Visibility = Visibility.Hidden;
      this._dlgSetTxtBoxTxt = new DlgSetTxtBoxTxt(setTxtBoxTxt);
    }

    private void textBox_PreviewDragOver(object sender, DragEventArgs e)
    {
      e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
    }

    private void procWitStt()
    {
      this.rctWait.Visibility = Visibility.Visible;
      this.LblWaitMess.Visibility = Visibility.Visible;
      this.intRptFlg = false;
      this.BtnGetLog.IsEnabled = false;
      this.BtnSort.IsEnabled = false;
      this.BtnClear.IsEnabled = false;
      this.checkBoxSvnExl.IsEnabled = false;
      this.textBoxExlSvn.IsEnabled = false;

      this.BtnIntRpt.IsEnabled = true;
    }

    private void procWitEnd()
    {
      this.rctWait.Visibility = Visibility.Hidden;
      this.LblWaitMess.Visibility = Visibility.Hidden;
      this.intRptFlg = false;
      this.BtnGetLog.IsEnabled = true;
      this.BtnSort.IsEnabled = true;
      this.BtnClear.IsEnabled = true;
      this.checkBoxSvnExl.IsEnabled = true;
      this.textBoxExlSvn.IsEnabled = true;

      this.BtnIntRpt.IsEnabled = false;
    }

    private void textBox_Drop(object sender, DragEventArgs e)
    {
      procWitStt();

      var txtBoxTxt = textBox.Text;
      var svnExlBool = checkBoxSvnExl.IsChecked;

      Thread t = new Thread(new ThreadStart(() =>
      {
        var addStr = new StringBuilder();
        var paths = e.Data.GetData(DataFormats.FileDrop) as string[];

        if (txtBoxTxt.Length > 0)
          addStr.Append(txtBoxTxt);

        if (txtBoxTxt.Length > 1)
          if (txtBoxTxt.Substring(txtBoxTxt.Length - 2, 2) != Environment.NewLine)
            addStr.Append(Environment.NewLine);

        foreach (var p in paths)
        {
          if (this.intRptFlg)
          {
            /*
            Dispatcher.Invoke(new Action(() =>
            {
              this.textBox.Text = txtBoxTxt;
              procWitEnd();
            }), new object[] { });
            */
            Dispatcher.Invoke(
              this._dlgSetTxtBoxTxt,
              new object[] { txtBoxTxt });
            return;
          }
          if (!File.Exists(p) && Directory.Exists(p))
          {
            var flLst = Directory.GetFiles(p, @"*", SearchOption.AllDirectories).Select(f => f);
            if (svnExlBool == true)
              flLst = flLst.Where(f => !f.Contains(@"\.svn\")).Select(f => f);
            foreach (var f in flLst)
            {
              addStr.Append(f);
              addStr.Append(Environment.NewLine);
            }
          }
          else
          {
            addStr.Append(p);
            addStr.Append(Environment.NewLine);
          }
        }
        /*
        Dispatcher.Invoke(new Action(() =>
        {
          this.textBox.Text = addStr.ToString();
          procWitEnd();
        }), new object[] { });
        */
        Dispatcher.Invoke(
          this._dlgSetTxtBoxTxt,
          new object[] { addStr.ToString() });
      }));
      t.Start();
     
    }

    private void BtnGetLog_Click(object sender, RoutedEventArgs e)
    {
      procWitStt();

      var txt = textBox.Text;
      Thread t = new Thread(new ThreadStart(() =>
      {
        var spNl = new string[] { Environment.NewLine };
        var spTb = new string[] { "\t" };

        var fls = txt.Split(spNl, StringSplitOptions.None)
          .Select(s => s.Split(spTb, StringSplitOptions.None)[0]);
        var sb = new StringBuilder();
        foreach (var fl in fls)
        {
          if (this.intRptFlg)
          {
            /*
            Dispatcher.Invoke(new Action(() =>
            {
              this.textBox.Text = txt;
              procWitEnd();
            }), new object[] { });
            */
            Dispatcher.Invoke(
              this._dlgSetTxtBoxTxt,
              new object[] { txt });
            return;
          }

          if (fl.Length > 0)
          {
            var cfo = new CustomFileInfo(fl);
            sb.Append(string.Format("{0}\t{1}\t{2}\t{3}{4}"
              , cfo.path, cfo.fileName, cfo.lastWriteTime, cfo.hashOfFileContent, Environment.NewLine));
          }
          else
          {
            sb.Append(Environment.NewLine);
          }
        }
        /*
        Dispatcher.Invoke(new Action(() =>
        {
          this.textBox.Text = sb.ToString();
          procWitEnd();
        }), new object[] { });
        */
        Dispatcher.Invoke(
          this._dlgSetTxtBoxTxt,
          new object[] { sb.ToString() });
      }));
      t.Start();
    }

    private void BtnSort_Click(object sender, RoutedEventArgs e)
    {
      procWitStt();

      var txtBoxTxt = textBox.Text;
      Thread t = new Thread(new ThreadStart(() =>
      {
        var spNl = new string[] { Environment.NewLine };
        var spTb = new string[] { "\t" };
        var tfm = txtBoxTxt.Split(spNl, StringSplitOptions.None).Where(l => l.Trim().Length > 0).Select(l =>
        {
          var ln = l.Split(spTb, StringSplitOptions.None);
          return new
          {
            path = ln.Length > 0 ? ln[0] : "",
            flNm = ln.Length > 1 ? ln[1] : "",
            ltTm = ln.Length > 2 ? ln[2] : "",
            hash = ln.Length > 3 ? ln[3] : ""
          };
        });
        var srt = tfm.OrderBy(o => o.path).Select(o =>
          string.Format("{0}\t{1}\t{2}\t{3}{4}", o.path, o.flNm, o.ltTm, o.hash, Environment.NewLine)
          );
        var addStr = new StringBuilder();
        foreach (var s in srt)
        {
          if (this.intRptFlg)
          {
            /*
            Dispatcher.Invoke(new Action(() =>
            {
              this.textBox.Text = txtBoxTxt;
              procWitEnd();
            }), new object[] { });
            */
            Dispatcher.Invoke(
              this._dlgSetTxtBoxTxt,
              new object[] { txtBoxTxt });
            return;
          }
          addStr.Append(s);
        }
        /*
        Dispatcher.Invoke(new Action(() =>
        {
          this.textBox.Text = addStr.ToString();
          procWitEnd();
        }), new object[] { });
        */
        Dispatcher.Invoke(
          this._dlgSetTxtBoxTxt,
          new object[] { addStr.ToString() });
      }));
      t.Start();
    }

    private void textBoxExlSvn_MouseDown(object sender, MouseButtonEventArgs e)
    {
      checkBoxSvnExl.IsChecked = !checkBoxSvnExl.IsChecked;
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
      textBox.Text = @"";
    }

    private void BtnIntRpt_Click(object sender, RoutedEventArgs e)
    {
      this.intRptFlg = true;
    }
  }
}
