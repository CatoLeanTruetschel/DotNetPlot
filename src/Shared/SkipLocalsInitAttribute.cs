/* License
 * --------------------------------------------------------------------------------------------------------------------
 * (C) Copyright 2021 Cato Léan Trütschel and contributors (https://github.com/CatoLeanTruetschel/DotNetPlot)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * --------------------------------------------------------------------------------------------------------------------
 */

#if !HAS_SKIP_LOCALS_INIT_ATTRIBUTE

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Constructor
        | AttributeTargets.Event
        | AttributeTargets.Interface
        | AttributeTargets.Method
        | AttributeTargets.Module
        | AttributeTargets.Property
        | AttributeTargets.Struct, Inherited = false)]
    internal sealed class SkipLocalsInitAttribute : Attribute { }
}

#endif
