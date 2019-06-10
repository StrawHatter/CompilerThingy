//===Paul Dreczkowski 1301428
using System;
using AllanMilne.Ardkit;


namespace AllanMilne.PALCompiler {


public class PALSemantics : Semantics {

	//--- The constructor method.
	public PALSemantics (IParser parser)
	: base (parser)
	{ }

	//--- Declare an identifier.
	public void DeclareId (IToken id) {
		if (!id.Is (Token.IdentifierToken)) return;	// only proceed if an identifier.
		Scope symbols = Scope.CurrentScope;
		if (symbols.IsDefined (id.TokenValue)) {
			semanticError (new AlreadyDeclaredError (id, symbols.Get(id.TokenValue)));
		} else {
			symbols.Add (new VarSymbol (id, currentType));
		}
	} // end DeclareId method.

	//--- Check the usage of an identifier.
	public int CheckId (IToken id) {
		if (!Scope.CurrentScope.IsDefined (id.TokenValue)) {
			semanticError (new NotDeclaredError (id));
			return LanguageType.Undefined;
		}
		else return(CheckType (id));	// check type compatibility.
	} // end checkId method.

	//--- Check type compatibility of current token.
	//--- Side-effect: sets current type if it is undefined at present.
	//--- Must only be called for identifiers that are declared.
	public int CheckType (IToken token) {
		int thisType = LanguageType.Undefined;
		if (token.Is (Token.IdentifierToken)){
			thisType = Scope.CurrentScope.Get (token.TokenValue).Type;
			return thisType;
		}
		else if (token.Is (Token.IntegerToken)){
			thisType = LanguageType.Integer;
			return thisType;
		}
		else if (token.Is (Token.RealToken)){
				thisType = LanguageType.Real;
				return thisType;
		}
		// if not already set then set the current type being processed.
		if (currentType == LanguageType.Undefined){
			currentType = thisType;
			return thisType;
		}
		if (currentType != thisType) {
			semanticError (new TypeConflictError (token, thisType, currentType));
		}
		
		return thisType;
		
	} // end checkType method.
	
	public bool checkMatch(IToken token, int left, int right){
		if (left != right){
			semanticError(new TypeConflictError(token, left, right));
			return false;
			}
		return true;
	}
} 

} 
