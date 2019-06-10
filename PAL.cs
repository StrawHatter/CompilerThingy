//===Paul Dreczkowski 1301428
using System;
using System.IO;
using System.Collections.Generic;

using AllanMilne.Ardkit;
using AllanMilne.PALCompiler;

//=== The main class and application entry point.
class PAL {

   private static ComponentInfo info = new ComponentInfo (
      "PAL", "1.0", "April 2019", "Paul Dreczkowski", "Compiler for the PAL language");
   public static ComponentInfo Info
   { get { return info; } }

   //=== the compiler entry point.
   public static void Main (String[] args) {

      if (args.Length != 1) {
         Console.WriteLine ("Invalid usage: PAL <filename>");
         return;
      }

      //--- Open the input source file.
      StreamReader infile   = null;
      try {
         infile = new StreamReader (args[0]);
      } catch (IOException e) { ioError ("opening", args[0], e); return; }

      //--- Do what you gotta do!
      PAL program = new PAL (infile);
      program.start();

      try {
         infile.Close();
      } catch (IOException e) { ioError ("closing", args[0], e); return; }

   } // end Main method.


   //--- The instance attributes.
   private TextReader source;    // the input source file stream.

   //--- constructor method.
   public PAL (TextReader src)
   { source = src; }

   //--- The Compiler body.
   private void start () {
      PALParser parser = new PALParser ();

      parser.Parse (source);

      foreach (ICompilerError err in parser.Errors) {
         Console.WriteLine (err);
      }

   } // end start method.

   //--- An I/O exception has been caught.
   private static void ioError (String function, String filename, IOException e) {
      Console.WriteLine ("An I/O error occurred {0:s} file {1:s}.", function, filename);
      Console.WriteLine (e);
   } // end ioError method.

}
