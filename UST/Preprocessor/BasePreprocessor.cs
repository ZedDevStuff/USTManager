using System.Collections.Generic;
using UnityEngine;

namespace USTManager.Preprocessor
{
    /// <summary>
    /// Inherit this class to create a new preprocessor. Rename the target clips from the list in the Apply method.
    /// </summary>
    public abstract class BasePreprocessor
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Format { get; }

        public abstract void Apply(IEnumerable<AudioSource> sources);
    }
}