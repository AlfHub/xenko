﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This value converter will convert any numeric value to integer. <see cref="ConvertBack"/> is supported and
    /// will convert the value to the target if it is numeric, otherwise it returns the value as-is.
    /// </summary>
    public class ToInteger : ValueConverterBase<ToInteger>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return typeof(long).CastToNumericType(value);
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !targetType.IsNumeric() ? value : targetType.CastToNumericType(value);
        }
    }
}
