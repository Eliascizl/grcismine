﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GuiSupport
{
  public class FormResolution : Form
  {
    private Label labelWid, labelHei;
    private NumericUpDown numericWid, numericHei;
    private Button buttonOk;
    private ToolTip toolTip;

    public int ImageWidth
    {
      get;
      set;
    }

    public int ImageHeight
    {
      get;
      set;
    }

    /// <summary>
    /// Output value: label for a "Resolution" button.
    /// </summary>
    public string Label;

    public FormResolution ( int initWid, int initHei )
    {
      ImageWidth  = initWid;
      ImageHeight = initHei;
      InitializeComponent();
    }

    private void buttonOk_Click ( object sender, EventArgs e )
    {
      ImageWidth  = (int)numericWid.Value;
      ImageHeight = (int)numericHei.Value;
      if ( ImageWidth == 0 && ImageHeight == 0 )
        Label = "Resolution";
      else
        Label = String.Format( "{0} x {1}",
                              (ImageWidth == 0)  ? "res" : ImageWidth.ToString(),
                              (ImageHeight == 0) ? "res" : ImageHeight.ToString() );
    }

    private void InitializeComponent ()
    {
      labelWid   = new Label();
      numericWid = new NumericUpDown();
      labelHei   = new Label();
      numericHei = new NumericUpDown();
      buttonOk   = new Button();
      toolTip    = new ToolTip();

      ((ISupportInitialize)numericWid).BeginInit();
      ((ISupportInitialize)numericHei).BeginInit();
      SuspendLayout();

      // labelWid
      labelWid.AutoSize = true;
      labelWid.Location = new System.Drawing.Point( 10, 16 );
      labelWid.Name = "labelWid";
      labelWid.Size = new System.Drawing.Size( 48, 13 );
      labelWid.TabIndex = 1;
      labelWid.Text = "Width:";

      // numericWid
      numericWid.Location = new Point( 70, 14 );
      numericWid.Maximum = new decimal( new int[] { 40000, 0, 0, 0 } );
      numericWid.Name = "numericWid";
      numericWid.Size = new Size( 100, 20 );
      numericWid.TabIndex = 2;
      numericWid.Value = new decimal( new int[] { ImageWidth, 0, 0, 0 } );

      // labelHei
      labelHei.AutoSize = true;
      labelHei.Location = new System.Drawing.Point( 10, 47 );
      labelHei.Name = "labelHei";
      labelHei.Size = new System.Drawing.Size( 48, 13 );
      labelHei.TabIndex = 3;
      labelHei.Text = "Height:";

      // numericHei
      numericHei.Location = new Point( 70, 45 );
      numericHei.Maximum = new decimal( new int[] { 30000, 0, 0, 0 } );
      numericHei.Name = "numericWid";
      numericHei.Size = new Size( 100, 20 );
      numericHei.TabIndex = 4;
      numericHei.Value = new decimal( new int[] { ImageHeight, 0, 0, 0 } );

      // buttonOk
      buttonOk.Location = new Point( 10, 80 );
      buttonOk.Name = "buttonOk";
      buttonOk.Size = new Size( 160, 23 );
      buttonOk.TabIndex = 5;
      buttonOk.Text = "Ok";
      buttonOk.UseVisualStyleBackColor = true;
      buttonOk.DialogResult = DialogResult.OK;
      buttonOk.Click += new System.EventHandler( buttonOk_Click );

      // FormResolution
      AutoScaleDimensions = new SizeF( 6f, 13f );
      AutoScaleMode = AutoScaleMode.Font;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      StartPosition = FormStartPosition.CenterParent;
      ClientSize = new Size( 182, 115 );
      Controls.Add( labelWid );
      Controls.Add( numericWid );
      Controls.Add( labelHei );
      Controls.Add( numericHei );
      Controls.Add( buttonOk );
      Name = "FormResolution";
      Text = "Resolution in pixels";

      ((ISupportInitialize)numericWid).EndInit();
      ((ISupportInitialize)numericHei).EndInit();
      ResumeLayout( false );

      toolTip.SetToolTip( numericWid, "Set 0 for default image size" );
      toolTip.SetToolTip( numericHei, "Set 0 for default image size" );

      PerformLayout();
    }
  }
}