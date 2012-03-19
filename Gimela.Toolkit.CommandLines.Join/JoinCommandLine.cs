﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Gimela.Toolkit.CommandLines.Foundation;

namespace Gimela.Toolkit.CommandLines.Join
{
  public class JoinCommandLine : CommandLine
  {
    #region Fields

    private JoinCommandLineOptions options;
    private const int BufferSize = 1024 * 20; // 20K

    #endregion

    #region Constructors

    public JoinCommandLine(string[] args)
      : base(args)
    {
    }

    #endregion

    #region ICommandLine Members

    public override void Execute()
    {
      base.Execute();

      List<string> singleOptionList = JoinOptions.GetSingleOptions();
      CommandLineOptions cloptions = CommandLineParser.Parse(Arguments.ToArray<string>(), singleOptionList.ToArray());
      options = ParseOptions(cloptions);
      CheckOptions(options);

      if (options.IsSetHelp)
      {
        RaiseCommandLineUsage(this, JoinOptions.Usage);
      }
      else if (options.IsSetVersion)
      {
        RaiseCommandLineUsage(this, JoinOptions.Version);
      }
      else
      {
        StartJoin();
      }

      Terminate();
    }

    #endregion

    #region Private Methods

    [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Gimela.Toolkit.CommandLines.Join.JoinCommandLine.OutputText(System.String)")]
    private void StartJoin()
    {
      try
      {
        string outputFile = WildcardCharacterHelper.TranslateWildcardFilePath(options.OutputFile);
        FileInfo outputFileInfo = new FileInfo(outputFile);
        if (outputFileInfo.Exists)
        {
          throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
            "Output file is already existent -- {0}", outputFile));
        }
        outputFileInfo.Directory.Create();

        IList<string> inputFiles = new List<string>();
        foreach (var item in options.InputFiles)
        {
          string inputFile = WildcardCharacterHelper.TranslateWildcardFilePath(item);
          if (!File.Exists(inputFile))
          {
            throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
              "Input file is non-existent -- {0}", inputFile));
          }

          inputFiles.Add(inputFile);
        }

        DateTime beginTime = DateTime.Now;
        JoinFile(inputFiles, outputFile);
        OutputText(string.Format(CultureInfo.CurrentCulture, "Output File : {0}", outputFile));
        DateTime endTime = DateTime.Now;
        OutputText(string.Format(CultureInfo.CurrentCulture, "Total Time  : {0}s", (endTime - beginTime).TotalSeconds));
      }
      catch (CommandLineException ex)
      {
        RaiseCommandLineException(this, ex);
      }
    }

    private void JoinFile(IList<string> inputFiles, string outputFile)
    {
      try
      {
        using (Stream output = File.Create(outputFile))
        {
          foreach (var inputFile in inputFiles)
          {
            int bytesRead = 0;
            byte[] buffer = new byte[BufferSize];
            using (Stream input = File.OpenRead(inputFile))
            {
              while ((bytesRead = input.Read(buffer, 0, BufferSize)) > 0)
              {
                output.Write(buffer, 0, bytesRead);
              }
            }
          }
        }
      }
      catch (DirectoryNotFoundException ex)
      {
        throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
          "Write output file failed -- {0}, {1}", options.OutputFile, ex.Message));
      }
      catch (PathTooLongException ex)
      {
        throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
          "Write output file failed -- {0}, {1}", options.OutputFile, ex.Message));
      }
      catch (IOException ex)
      {
        throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
          "Write output file failed -- {0}, {1}", options.OutputFile, ex.Message));
      }
      catch (NotSupportedException ex)
      {
        throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
          "Write output file failed -- {0}, {1}", options.OutputFile, ex.Message));
      }
      catch (UnauthorizedAccessException ex)
      {
        throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
          "Write output file failed -- {0}, {1}", options.OutputFile, ex.Message));
      }
    }

    #endregion

    #region Parse Options

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static JoinCommandLineOptions ParseOptions(CommandLineOptions commandLineOptions)
    {
      if (commandLineOptions == null)
        throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
          "Option used in invalid context -- {0}", "must specify a option."));

      JoinCommandLineOptions targetOptions = new JoinCommandLineOptions();

      if (commandLineOptions.Arguments.Count >= 0)
      {
        foreach (var arg in commandLineOptions.Arguments.Keys)
        {
          JoinOptionType optionType = JoinOptions.GetOptionType(arg);
          if (optionType == JoinOptionType.None)
            throw new CommandLineException(
              string.Format(CultureInfo.CurrentCulture, "Option used in invalid context -- {0}",
              string.Format(CultureInfo.CurrentCulture, "cannot parse the command line argument : [{0}].", arg)));

          switch (optionType)
          {
            case JoinOptionType.OutputFile:
              targetOptions.OutputFile = commandLineOptions.Arguments[arg];
              break;
            case JoinOptionType.Help:
              targetOptions.IsSetHelp = true;
              break;
            case JoinOptionType.Version:
              targetOptions.IsSetVersion = true;
              break;
          }
        }
      }

      if (commandLineOptions.Parameters.Count > 0)
      {
        foreach (var item in commandLineOptions.Parameters)
        {
          targetOptions.InputFiles.Add(item);
        }
      }

      return targetOptions;
    }

    private static void CheckOptions(JoinCommandLineOptions checkedOptions)
    {
      if (!checkedOptions.IsSetHelp && !checkedOptions.IsSetVersion)
      {
        if (string.IsNullOrEmpty(checkedOptions.OutputFile))
        {
          throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
            "Option used in invalid context -- {0}", "must specify a output file path."));
        }
        if (checkedOptions.InputFiles.Count <= 1)
        {
          throw new CommandLineException(string.Format(CultureInfo.CurrentCulture,
            "Option used in invalid context -- {0}", "should specify two or more input files."));
        }
      }
    }

    #endregion
  }
}
