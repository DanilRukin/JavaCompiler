using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.Common
{
    /// <summary>
	/// Набор лексем языка, таких как точка, запятая, ключевые слова, и т.д.
	/// </summary>
    public enum Lexemes
    {
        /// <summary>
        /// class
        /// </summary>
        TypeClassKeyWord,

        /// <summary>
        /// while
        /// </summary>
        TypeWhileKeyWord,

        /// <summary>
        /// Например, 14,5
        /// </summary>
        TypeDoubleLiteral,

        /// <summary>
        /// Например, 8
        /// </summary>
        TypeIntLiteral,

        /// <summary>
        /// true, false
        /// </summary>
        TypeBooleanLiteral,

        /// <summary>
        /// null
        /// </summary>
        TypeNullLiteral,

        /// <summary>
        /// return
        /// </summary>
        TypeReturnKeyWord,

        /// <summary>
        /// final
        /// </summary>
        TypeFinalKeyWord,

        /// <summary>
        /// int
        /// </summary>
        TypeIntKeyWord,

        /// <summary>
        /// double
        /// </summary>
        TypeDoubleKeyWord,

        /// <summary>
        /// boolean
        /// </summary>
        TypeBooleanKeyWord,

        /// <summary>
        /// Например, MyAnimal
        /// </summary>
        TypeIdentifier,

        /// <summary>
        /// &lt;
        /// </summary>
        TypeLessSign,

        /// <summary>
        /// &lt;=
        /// </summary>
        TypeLessOrEqualSign,

        /// <summary>
        /// &gt;
        /// </summary>
        TypeMoreSign,

        /// <summary>
        /// &gt;=
        /// </summary>
        TypeMoreOrEqualSign,

        /// <summary>
        /// ==
        /// </summary>
        TypeEqualSign,

        /// <summary>
        /// !=
        /// </summary>
        TypeNotEqualSign,

        /// <summary>
        /// =
        /// </summary>
        TypeAssignmentSign,

        /// <summary>
        /// +
        /// </summary>
        TypePlus,

        /// <summary>
        /// -
        /// </summary>
        TypeMinus,

        /// <summary>
        /// ++
        /// </summary>
        TypeIncrement,

        /// <summary>
        /// --
        /// </summary>
        TypeDecrement,

        /// <summary>
        /// *
        /// </summary>
        TypeMult,

        /// <summary>
        /// /
        /// </summary>
        TypeDiv,

        /// <summary>
        /// %
        /// </summary>
        TypeMod,

        /// <summary>
        /// .
        /// </summary>
        TypeDot,

        /// <summary>
        /// ;
        /// </summary>
        TypeSemicolon,

        /// <summary>
        /// {
        /// </summary>
        TypeOpenCurlyBrace,

        /// <summary>
        /// }
        /// </summary>
        TypeCloseCurlyBrace,

        /// <summary>
        /// (
        /// </summary>
        TypeOpenParenthesis,

        /// <summary>
        /// )
        /// </summary>
        TypeCloseParenthesis,

        /// <summary>
        /// ,
        /// </summary>
        TypeComma,

        TypeError,

        /// <summary>
        /// #
        /// </summary>
        TypeEnd,
    }
}
