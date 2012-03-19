﻿using Gimela.Toolkit.CommandLines.Foundation;

namespace Gimela.Toolkit.CommandLines.AddText
{
  class Program
  {
    static void Main(string[] args)
    {
      using (CommandLine command = new AddTextCommandLine(args))
      {
        CommandLineBootstrap.Start(command);
      }
    }
  }
}
