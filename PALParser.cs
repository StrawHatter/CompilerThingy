//===Paul Dreczkowski 1301428
using System;
using System.IO;
using System.Collections.Generic;

using AllanMilne.Ardkit;

namespace AllanMilne.PALCompiler {

//=== the parser class.
public class PALParser : RecoveringRdParser {
	
   private PALSemantics semantics;

   //--- The constructor method.
   public PALParser () 
   : base (new PALScanner())
   { 
      semantics = new PALSemantics (this);
   }

   
   // <Program>
   protected override void recStarter () {
	  Scope.OpenScope ();
	  mustBe ("PROGRAM");
	  mustBe(Token.IdentifierToken);
	  mustBe ("WITH");
      recDeclarations();
	  mustBe ("IN");
      recStatementList();
	  mustBe("END");
	  Scope.CloseScope ();
   }
   
   
   // <VarDecls>
   private void recDeclarations () {
	  List<IToken> tokens = new List<IToken>();
	  
	  if (have(Token.IdentifierToken)){
		  
		  tokens = recIdentifierList();
		  mustBe ("AS");
		  recType();
		  foreach(IToken token in tokens){
			  semantics.DeclareId(token);
		  }
		  recDeclarations();
	  }
      else {} 
   }



   // <IdentList>
   private List<IToken> recIdentifierList () {
	  List<IToken> tokens = new List<IToken>();
	  tokens.Add(scanner.CurrentToken);
	  mustBe(Token.IdentifierToken);
      while (have(",")) {
		  mustBe(",");
          tokens.Add(scanner.CurrentToken);
		  mustBe(Token.IdentifierToken);
      }
	  return tokens;
   }

   // (<Statement>)+
   private void recStatementList () {
      if (have(Token.IdentifierToken) || have("UNTIL") ||
      have("IF") || have("INPUT") || have("OUTPUT")) {
        recStatement();
		if (have(Token.IdentifierToken) || have("UNTIL") ||
        have("IF") || have("INPUT") || have("OUTPUT")) {
			recStatementList();
		}
      }
      else syntaxError("<Statement>");
   }
   

   // <Statement>
   private void recStatement () {
      if (have(Token.IdentifierToken))       recAssignment();
      else if (have("UNTIL"))  recLoop();
      else if (have("IF"))	recConditional();
      else if (have("INPUT") || have("OUTPUT"))	recIO();
      else
         syntaxError ("<Malformed Statement>");
   }

   // <Assignment>
   private void recAssignment () {
	  IToken token = scanner.CurrentToken;
	  int left = semantics.CheckId(token);
      mustBe (Token.IdentifierToken);
      mustBe ("=");
      int right = recExpression();
	  semantics.checkMatch(token, left, right);
   }

   // <Loop>
   private void recLoop() {
	   mustBe("UNTIL");
	   recBooleanExpr();
	   mustBe("REPEAT");
		if (have(Token.IdentifierToken) || have("UNTIL") ||
    	have("IF") || have("INPUT") || have("OUTPUT")) {
			recStatementList();
		}
	   mustBe("ENDLOOP");
	  
   }
   
   
   // <Conditional>
   private void recConditional () {
      mustBe ("IF");
      recBooleanExpr();
      mustBe ("THEN");
      if (have(Token.IdentifierToken) || have("UNTIL") ||
      have("IF") || have("INPUT") || have("OUTPUT")) {
	    recStatementList();
	  }
      if (have("ELSE")){
		  mustBe("ELSE");
		  if (have(Token.IdentifierToken) || have("UNTIL") ||
          have("IF") || have("INPUT") || have("OUTPUT")) {
			recStatementList();
		}
	  }
      mustBe ("ENDIF");
   }
   

   // <I-o>
   private void recIO() {
	   if (have("INPUT")){
	      mustBe("INPUT");
		  recIdentifierList();
	   }
	   else if (have("OUTPUT")) {
		   mustBe("OUTPUT");
		   recExpression();
		   while (have(",")) {
			mustBe(",");
			recExpression();
			}
	   }
   }
   
   // <BooleanExpr>
   private void recBooleanExpr(){
	   IToken token = scanner.CurrentToken;
	   int left = recExpression();
	   if (have("<")){
		   mustBe("<");
	   }
	   else if (have("=")){
		   mustBe("=");
	   }
	   else if (have(">")){
		   mustBe(">");
	   }
	   else syntaxError("<BooleanExpr>");
	   int right = recExpression();
	   semantics.checkMatch(token, left, right);
   }
   
   // <Expression> ::= <Term> ___
   private int recExpression () {
	  IToken token = scanner.CurrentToken;
      int left = recTerm();
      int right = recRestExpr();
	  if (right == 99) return left;
	  if(semantics.checkMatch(token, left, right)){
		  return left;
	  }
	  return right;
   }
   
   // <Expression> ::= __ ( (+|-) <Term>)*
   private int recRestExpr () {
	  IToken token;
	  int left, right;
      if (have("+")) {
         mustBe ("+"); token = scanner.CurrentToken;  left = recTerm(); right = recRestExpr();
      }
      else if (have("-")) {
         mustBe ("-");  token = scanner.CurrentToken; left = recTerm();  right = recRestExpr();
      }
      else {token = scanner.CurrentToken; left = 99; right = 99;}   // do nothing - null production.
	  if(right == 99) return left;
	  if(semantics.checkMatch(token, left, right)){
		  return left;
	  }
	  return right;
   }

   // <Term> ::= <Factor> ___
   private int recTerm() {
	  IToken token = scanner.CurrentToken;
      int left = recFactor();
      int right = recRestTerm();
	  if (right == 99) return left;
	  if(semantics.checkMatch(token, left, right)){
		  return left;
	  }
	  return right;
   }

   // <Term> ::= ___ ( (*|/) <Factor>)*
   private int recRestTerm () {
	  IToken token;
	  int left, right;
      if (have("*"))  {
         mustBe ("*"); token = scanner.CurrentToken; left = recFactor(); right = recRestTerm();
      }
      else if (have("/")) {
         mustBe ("/");  token = scanner.CurrentToken; left = recFactor(); right = recRestTerm();
      }
      else {token = scanner.CurrentToken; left = 99; right = 99;}
	  if (right == 99) return left;
	  if(semantics.checkMatch(token, left, right)){
		  return left;
	  }
	  return right;
   }

   // <Factor>
   private int recFactor () {
	  
	  int type;
	  if (have("+")) {
		  mustBe("+");
	  }
	  else if (have("-")) {
		  mustBe("-");
	  }
	
      if (have(Token.IdentifierToken)) {
		 type = semantics.CheckId(scanner.CurrentToken);
         mustBe (Token.IdentifierToken);
	
      }
      else if (have(Token.IntegerToken)) {
		 type = LanguageType.Integer;
         mustBe (Token.IntegerToken);
      }
      else if (have(Token.RealToken)) {
		 type = LanguageType.Real;
         mustBe (Token.RealToken);
      }
      else if (have("(")) {
         mustBe ("(");
         type = recExpression();
         mustBe (")");
      }
      else{
		 type = LanguageType.Undefined;
         syntaxError ("<Factor>");
	  }
	 
	  return type;
   }

   // <Type>
   private void recType () {
      if (have("REAL")) {
         mustBe ("REAL");
		 semantics.CurrentType = LanguageType.Real;
      }
      else if (have("INTEGER")) {
         mustBe ("INTEGER");
		 semantics.CurrentType = LanguageType.Integer;
      }
	  else syntaxError("<Type>");
   }
   
}

}
