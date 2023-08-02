using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.Common
{
    /// <summary>
    /// Перечисление всех нетерминалов управляющей таблицы
    /// </summary>
    public enum NonTerminals
    {
        CompilationUnit,
        TypeDeclarations,
        ClassDeclaration,
        ClassBody,
        ClassBodyDeclaration,
        ClassMemberDeclaration,
        FieldDeclaration,
        VariableDeclaratorList,
        VariableDeclaratorList_1,
        VariableDeclarator,
        MethodDeclaration,
        MethodHeader,
        ResultType,
        MethodDeclarator,
        MethodBody,
        ConstructorDeclaration,
        Block,
        BlockStatements,
        BlockStatement,
        LocalVariableDeclaration,
        Statement,
        ExpressionStatement,
        StatementExpression,
        WhileStatement,
        ReturnStatement,
        ClassInstanceCreationExpression,
        ClassInstanceCreationExpression_1,
        UnqualifiedClassInstanceCreationExpression,
        FieldAccess,
        Primary,
        MethodInvocation,
        PostfixExpression,
        PostDecrementExpression,
        PostIncrementExpression,
        UnaryExpression,
        PreIncrementExpression,
        PreDecrementExpression,
        UnaryExpressionNotPlusMinus,
        CastExpression,
        MultiplicativeExpression,
        MultiplicativeExpression_1,
        AdditiveExpression,
        AdditiveExpression_1,
        RelationalExpression,
        RelationalExpression_1,
        EqualityExpression,
        EqualityExpression_1,
        AssignmentExpression,
        Assignment,
        LeftHandSide,
        AssignmentOperator,
        Expression,
        ConstantExpression,
        Literal,
        IntegerLiteral,
        Digits,
        Digit,
        NonZeroDigit,
        FloatingPointLiteral,
        BooleanLiteral,
        NullLiteral,
        Type,
        PrimitiveType,
        NumericType,
        ReferenceType,
        ClassType,
        TypeName,
        TypeName_1,
        ExpressionName,
        ExpressionName_1,
        MethodName,

        /// <summary>
        /// Данный нетерминал не принадлежит таблице
        /// </summary>
        Invalid,
        Identifier
    }
}
