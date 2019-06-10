/*
	File: PALScanner.cs
	Purpose: A scanner for the PAL language.
	Version: 1.1
	Date: 2010, 14 April 2011.
	Author: Allan C. Milne.

	Namespace: AllanMilne.PALCompiler
	Uses: Ardkit.dll
	Exposes: PALScanner.

	Description:
	The language PAL is defined in the file PAL.EBNF.txt.
	This scanner implementation is built on version 2 of the Ardkit compiler toolkit framework.
	This version of the scanner uses a state transition FSM algorithm.
	See 'PAL.FSM.txt' for the FSM.
	Invalid characters and tokens are identified and appropriate tokens returned.
*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AllanMilne.Ardkit;

namespace AllanMilne.PALCompiler {

//=== The PAL scanner implementation.
public class PALScanner : Scanner {

   // list of keywords used in the language - remember they are case-sensitive.
   private static List<String> keywords = new List<String> ( new String[] {
	"PROGRAM", "WITH", "IN", "END",
	"AS", "INTEGER", "REAL", "UNTIL", "REPEAT", "ENDLOOP",
	"IF", "THEN", "ELSE", "ENDIF", "INPUT", "OUTPUT"
   });

   //--- find and return the next token using a state transition FSM.
   protected override IToken getNextToken ()  {
      StringBuilder strbuf = null;             // for building actual token.
      int          state = 0;                 // always start at FSM state S0.
      int          startLine=0, startCol=0;   // start position of token.

Token token = null;
      while (token == null) {   // repeat until a token is found.
         switch (state) {
         case 0:   // start state.
            if (Char.IsWhiteSpace(currentChar))  state = 0;
            else {
            startLine = line;   startCol = column;    // start of actual token.
            strbuf =new StringBuilder();              // for actual token.

            if (Char.IsLetter (currentChar))                    state = 1;
            else if (Char.IsDigit (currentChar))                state = 2;
            else if ("+-*/(),=<>".IndexOf(currentChar) != -1)   state = 4;
            else if (currentChar == eofChar)                    state = 98;
            else                                                state = 99;
            }
            break;

         case 1:   // Identifier or keyword state.
            if (Char.IsLetter(currentChar) ||
                Char.IsDigit(currentChar) )
                 state = 1;
            else {                                  // identifier might be a keyword.
                 String s = strbuf.ToString();      // case-sensitive, so don't change case.
                 if (keywords.Contains (s))         // if is a keyword.
                      token = new Token (s, startLine, startCol);
                 else token = new Token (Token.IdentifierToken, s, startLine, startCol);
            }
            break;

         case 2:   // start of integer or real.
            if (Char.IsDigit (currentChar))   state = 2;
            else if (currentChar == '.')      state = 3;
            else
               token = new Token (Token.IntegerToken, strbuf.ToString(), startLine, startCol);
            break;

         case 3:   // remainder of a real number.
            if (Char.IsDigit (currentChar))
                 state = 3;
            else token = new Token (Token.RealToken, strbuf.ToString(), startLine, startCol);
            break;

         case 4:   // single character punctuation.
            token = new Token (strbuf.ToString(), startLine, startCol);
            break;

         case 98:   // end-of-file
            token = new Token (Token.EndOfFile, startLine, startCol);
            break;
         case 99:   // invalid-char
            token = new Token (Token.InvalidChar, strbuf.ToString(), startLine, startCol);
            break;
         }

         if (token == null) {                 // have not yet found the token.
            if (state != 0) {                 // not in state 0, so not whitespace.
               strbuf.Append (currentChar);   // build up token string.
            }
            getNextChar();                    // char has been used so get next one.
         }
      } // end while not found token.

      return token;

   } // end getNextToken method.

} // end PALScanner class.

} // end namespace.
