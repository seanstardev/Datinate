using System.Text.RegularExpressions;
using static datinate.shared.DatFilterHelper;

namespace datinate.shared
{
    public sealed class DatFilter
    {
        private readonly string expression;
        private readonly EXPRESSION_ACTION_ENUM expressionActionEnum;

        private readonly string regexExpression;
        private readonly Regex regex;

        public DatFilter(string expression, EXPRESSION_ACTION_ENUM expressionActionEnum)
        {
            this.expression = expression;
            this.expressionActionEnum = expressionActionEnum;

            regexExpression = ExpressionHandler.Convert(expression);
            regex = new Regex(regexExpression, RegexOptions.Compiled);
        }

        public bool ExcludeAlways() => expressionActionEnum == EXPRESSION_ACTION_ENUM.EXCLUDE;

        public bool IncludeAlways() => expressionActionEnum == EXPRESSION_ACTION_ENUM.INCLUDE;

        public bool GetExcludeConditional() => expressionActionEnum == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL;

        public string GetUserFriendlyExpression() => expression;

        public string GetRegexExpression() => regexExpression;

        public EXPRESSION_ACTION_ENUM GetExpressionAction() => expressionActionEnum;

        public string GetActionName() => expressionActionEnum.ToString();

        public bool IsMatch(string target) => regex.IsMatch(target);

        public string ReplaceMatches(string target, string replacement) => regex.Replace(target, replacement);
    }
}
