using System.Collections.Generic;
using Minigames.Swinging.Core;
using Xunit;

namespace Swinging.Core.Tests {
    public class Vec2Tests {

        public static IEnumerable<object[]> AdditionCases => new[]
        {
            new object[] { new Vec2(1, 1), new Vec2(1, 1), new Vec2(2, 2) },
            new object[] { new Vec2(1, 1), new Vec2(0, 0), new Vec2(1, 1) }
        };
        
        [Theory]
        [MemberData(nameof(AdditionCases))]
        public void VectorsAddCorrectly(Vec2 vector1, Vec2 vector2, Vec2 expected) {
            Assert.Equal(expected, vector1+vector2);
        }

        [Fact]
        public void VectorsConstructCorrectly() {
            Vec2 vector = new Vec2(1, 2);
            Assert.Equal(1, vector.X);
            Assert.Equal(2, vector.Y);
        }
        
        public static IEnumerable<object[]> SubtractionCases => new[]
        {
            new object[] { new Vec2(1, 1), new Vec2(1,1), new Vec2(0,0) },
            new object[] { new Vec2(1, 1), new Vec2(-1,1), new Vec2(2,0) }
        };

        [Theory]
        [MemberData(nameof(SubtractionCases))]
        public void VectorsSubtractCorrectly(Vec2 vector1, Vec2 vector2, Vec2 expected) {
            Assert.Equal(expected, vector1-vector2);
        }
        
        public static IEnumerable<object[]> VectorScalarMultiplicationCases => new[]
        {
            new object[] { new Vec2(1, 1), 1, new Vec2(1,1) },
            new object[] { new Vec2(1, 1), 2, new Vec2(2,2) }
        };
        
        [Theory]
        [MemberData(nameof(VectorScalarMultiplicationCases))]
        public void VectorsMultiplyCorrectly(Vec2 vector1, float scalar, Vec2 expected) {
            Assert.Equal(expected, vector1*scalar);
            Assert.Equal(expected, scalar*vector1);
        }
    }
}