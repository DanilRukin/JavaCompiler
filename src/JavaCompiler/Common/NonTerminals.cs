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
        ClassBodyDeclarations,
        ClassMemberDeclaration,
        FieldDeclaration,
        VariableDeclarator,
        VariableDeclarators,
        MethodDeclaration,
        MethodHeader,
        ResultType,
        MethodDeclarator,
        MethodBody,
        ConstructorDeclaration,
        ConstructorBody,
        Name,
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
        ClassInstanceCreationExpression_2,
        UnqualifiedClassInstanceCreationExpression,
        FieldAccess,
        FieldAccess_1,
        Primary,
        PrimaryExpression,
        PrimaryExpression_1,
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
        Expression_1,
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
        Identifier,
        PrefixExpression,
        PostfixExpression_1
    }
}
