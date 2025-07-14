using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDos.DotNet.Common;

namespace ToDos.DotNet.Common.Tests
{
    [TestClass]
    public class DtoTests
    {
        [TestMethod]
        public void TagDTO_CanBeConstructed()
        {
            var tag = new TagDTO { Id = System.Guid.NewGuid(), Name = "TestTag" };
            Assert.IsNotNull(tag.Id);
            Assert.AreEqual("TestTag", tag.Name);
        }

        [TestMethod]
        public void TaskDTO_Equality_Works()
        {
            var t1 = new TaskDTO { Id = 1, Title = "A" };
            var t2 = new TaskDTO { Id = 1, Title = "A" };
            Assert.AreEqual(t1.Id, t2.Id);
            Assert.AreEqual(t1.Title, t2.Title);
        }

        [TestMethod]
        public void TagDTO_Defaults_Are_Valid()
        {
            var tag = new TagDTO();
            Assert.AreEqual(System.Guid.Empty, tag.Id);
            Assert.IsNull(tag.Name);
        }
    }
} 