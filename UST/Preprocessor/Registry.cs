using System.Collections.Generic;

namespace USTManager.Preprocessor
{
    public static class Registry
    {
        internal static List<BasePreprocessor> Preprocessors;
        /// <summary>
        /// Returns true if the preprocessor was registered
        /// </summary>
        /// <param name="preprocessor"></param>
        /// <returns></returns>
        public static bool Register(BasePreprocessor preprocessor)
        {
            if(!Preprocessors.Contains(preprocessor))
            {
                Preprocessors.Add(preprocessor);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true if the preprocessor was unregistered
        /// </summary>
        /// <param name="preprocessor"></param>
        /// <returns></returns>
        public static bool UnRegister(BasePreprocessor preprocessor)
        {
            if(Preprocessors.Contains(preprocessor))
            {
                Preprocessors.Remove(preprocessor);
                return true;
            }
            return false;
        }
    }
}