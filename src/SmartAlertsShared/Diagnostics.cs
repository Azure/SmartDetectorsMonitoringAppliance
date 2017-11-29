namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class Diagnostics
    {
        /// <summary>
        /// A throwing assert to determine that an argument is non-null.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="variableExpression">The expression that returns the argument.
        /// For example:
        /// EnsureArgumentNotNull(() => filePath)
        /// EnsureArgumentNotNull(() => Policy.Id)
        /// </param>
        public static T EnsureArgumentNotNull<T>(Expression<Func<T>> variableExpression) where T : class
        {
            T value = variableExpression.Compile()();

            if (value == null)
            {
                // Getting the name of the variable
                var variableName = GetReturnValueAsString(variableExpression);
                throw new ArgumentNullException(variableName);
            }

            return value;
        }

        /// <summary>
        /// A throwing assert to determine that a string argument is non-null and non-whiteSpace
        /// </summary>
        /// <param name="variableExpression">The expression that returns the argument.
        /// For example:
        /// EnsureStringNotNullOrWhiteSpace(() => filePath)
        /// EnsureStringNotNullOrWhiteSpace(() => job.Name)
        /// </param>
        public static string EnsureStringNotNullOrWhiteSpace(Expression<Func<string>> variableExpression)
        {
            string value = variableExpression.Compile()();

            if (string.IsNullOrWhiteSpace(value))
            {
                // Getting the name of the variable
                var variableName = GetReturnValueAsString(variableExpression);
                throw new ArgumentNullException(variableName);
            }

            return value;
        }

        /// <summary>
        /// A throwing assert to determine that an argument is in the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="variableExpression">The expression that returns the argument.
        /// For example:
        /// EnsureArgumentInRange(() => threshold, 0, 1)
        /// </param>
        /// <param name="minValue">The range minimal value</param>
        /// <param name="maxValue">The range maximal value</param>
        public static T EnsureArgumentInRange<T>(Expression<Func<T>> variableExpression, T minValue, T maxValue) where T : IComparable
        {
            T value = variableExpression.Compile()();

            if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
            {
                // Getting the name of the variable
                var variableName = GetReturnValueAsString(variableExpression);
                throw new ArgumentException($"{variableName} failed range validation: {minValue} <= {variableName} <= {maxValue} does not hold");
            }

            return value;
        }

        /// <summary>
        /// A throwing assert to determine that a value meets a condition (will throw an ArgumentException without 
        /// an inputted message)
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="variableExpression">The Argument being validated</param>
        /// <param name="message">the exception message</param>
        public static T EnsureArgument<T>(bool condition, Expression<Func<T>> variableExpression, string message = null)
        {
            if (!condition)
            {
                // Getting the name of the variable
                var variableName = GetReturnValueAsString(variableExpression);
                throw (message == null) ? new ArgumentOutOfRangeException(variableName) : new ArgumentOutOfRangeException(variableName, message);
            }

            return variableExpression.Compile()();
        }

        /// <summary>
        /// This method should be invoked on expressions that return a local variable.
        /// If this method can't parse the return value, it will return "unknown".
        /// 
        /// Examples:
        /// For the expression  () => id                    the returned value will be "id".
        /// For the expression  () => Policy.Id             the returned value will be "Policy.Id".
        /// For the expression  () => "test".ToString()     the returned value will be "unknown".
        /// </summary>
        /// <param name="argumentExpression">The expression returning the local variable.</param>
        /// <returns>The local variable name (or "unknown")</returns>
        private static string GetReturnValueAsString(LambdaExpression argumentExpression)
        {
            const string unknownExpressionRepresentation = "unknown";

            if (argumentExpression == null)
            {
                return unknownExpressionRepresentation;
            }

            try
            {
                var nextExpression = argumentExpression.Body;

                var nestedNames = new List<string>();
                var nestingCounter = 0;

                // Getting the full variable path (given "Policy.Rule.Id" we will go through "Id", then "Rule" and then "Policy")
                while (nextExpression is MemberExpression)
                {
                    // Getting the current member
                    var memberExpression = nextExpression as MemberExpression;

                    // Saving this part
                    nestedNames.Insert(0, memberExpression.Member.Name);

                    // Getting the immediate parent expression (given "Policy.Rule.Id", "Rule" is the parent of "Id")
                    nextExpression = memberExpression.Expression;

                    // Making sure this is not an endless loop
                    nestingCounter++;
                    if (nestingCounter > 100)
                    {
                        break;
                    }
                }

                // Joining the parts to a full path
                string variableFullName = string.Join(".", nestedNames);
                return variableFullName;
            }
            catch (Exception)
            {
                return unknownExpressionRepresentation;
            }
        }
    }
}