﻿/***********************************************************************
Copyright 2016 Stefan Hausotte

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

*************************************************************************/

using Xunit;
using PeNet.Structures;

namespace PeNet.UnitTest.Structures
{
    
    public class IMAGE_BOUND_IMPORT_DESCRIPTOR_Test
    {
        [Fact]
        public void ImageBoundImportDescriptorConstructorWorks_Test()
        {
            var boundImportDescriptor = new IMAGE_BOUND_IMPORT_DESCRIPTOR(RawStructures.RawBoundImportDescriptor, 2);
            Assert.Equal((uint) 0x33221100, boundImportDescriptor.TimeDateStamp);
            Assert.Equal((ushort) 0x5544, boundImportDescriptor.OffsetModuleName);
            Assert.Equal((ushort) 0x7766, boundImportDescriptor.NumberOfModuleForwarderRefs);
        }
    }
}