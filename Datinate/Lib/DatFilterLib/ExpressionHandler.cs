using System.Text.RegularExpressions;

namespace datinate.shared
{
    public class ExpressionHandler {

        private readonly string expression;

        public ExpressionHandler(string expression) 
        {
            this.expression = Convert(@expression);
        }

        public static bool Match (string target, string quickExpression) 
        {
            quickExpression = Convert(quickExpression);
         
            if (Regex.IsMatch(target, quickExpression))
                return true;
            else
                return false;
        }

        /**
         * NOTE: Only one special caharcter should be included at this stage.
         */
        public static string Convert(string raw) {

            if (raw == "")
                return raw;

            // Zero or many of anything     .*
            // One or many numbers          [0-9]+

            bool endsWithSpecialCharacter = false;

            if (
                raw.EndsWith(DatFilterHelper.REGEX_ZERO_OR_MORE_OF_ANYTHING)
                    ||
                raw.EndsWith(DatFilterHelper.REGEX_ONE_OR_MORE_NUMBERS)
                    ||
                raw.EndsWith(DatFilterHelper.REGEX_ONE_OR_MORE_LETTERS)
            ) {
                endsWithSpecialCharacter = true;
            }

            bool endsWithSquare = raw.Length > 0 && raw[raw.Length - 1] == ']';
            bool endsWithRound = raw.Length > 0 && raw[raw.Length - 1] == ')';

            raw = Regex.Escape(raw);

            string anything = "\\" + DatFilterHelper.REGEX_ZERO_OR_MORE_OF_ANYTHING;

            if (raw.Contains(anything))
            {
                if (endsWithSquare)
                {
                    raw = raw.Replace(anything, @"[^\]]*");
                }
                else if (endsWithRound)
                {
                    raw = raw.Replace(anything, @"[^\)]*");
                }
                else
                {
                    raw = raw.Replace(anything, ".*");
                }
            }


            if (raw.Contains(DatFilterHelper.REGEX_ONE_OR_MORE_NUMBERS)) 
            {
                raw = raw.Replace(DatFilterHelper.REGEX_ONE_OR_MORE_NUMBERS, "[0-9]+");
            }

            if (raw.Contains(DatFilterHelper.REGEX_ONE_OR_MORE_LETTERS)) 
            {
                raw = raw.Replace(DatFilterHelper.REGEX_ONE_OR_MORE_LETTERS, "[a-zA-Z]+");
            }

            return raw;
        }
        
        public bool Match(string target) 
        {
            if (Regex.IsMatch(target, expression)) 
                return true;
            else 
                return false;
        }
    }
}
