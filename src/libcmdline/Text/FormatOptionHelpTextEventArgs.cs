namespace CommandLine.Text
{
    using System;

    /// <summary>
    /// Provides data for the FormatOptionHelpText event.
    /// </summary>
    public class FormatOptionHelpTextEventArgs : EventArgs
    {
        private readonly BaseOptionAttribute _option;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLine.Text.FormatOptionHelpTextEventArgs"/> class.
        /// </summary>
        /// <param name="option">Option to format.</param>
        public FormatOptionHelpTextEventArgs(BaseOptionAttribute option)
        {
            _option = option;
        }

        /// <summary>
        /// Gets the option to format.
        /// </summary>
        public BaseOptionAttribute Option
        {
            get
            {
                return _option;
            }
        }
    }
}