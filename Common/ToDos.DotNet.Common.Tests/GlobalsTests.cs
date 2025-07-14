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

        [TestMethod]
        public void TaskDTO_PropertyAssignment_Works()
        {
            var dto = new TaskDTO { Id = 42, Title = "Test Task", IsCompleted = true };
            Assert.AreEqual(42, dto.Id);
            Assert.AreEqual("Test Task", dto.Title);
            Assert.IsTrue(dto.IsCompleted);
        }

        [TestMethod]
        public void TagDTO_PropertyAssignment_Works()
        {
            var tag = new TagDTO { Id = Guid.NewGuid(), Name = "UnitTest" };
            Assert.IsNotNull(tag.Id);
            Assert.AreEqual("UnitTest", tag.Name);
        }

        [TestMethod]
        public void TaskDTO_Equality_And_Mutation()
        {
            var t1 = new TaskDTO { Id = 1, Title = "A" };
            var t2 = new TaskDTO { Id = 1, Title = "A" };
            Assert.AreEqual(t1.Id, t2.Id);
            Assert.AreEqual(t1.Title, t2.Title);
            t2.Title = "B";
            Assert.AreNotEqual(t1.Title, t2.Title);
        }

        [TestMethod]
        public void TaskDTO_Tags_CanBeAssignedAndMutated()
        {
            var t = new TaskDTO { Id = 1, Title = "A", Tags = new System.Collections.Generic.List<TagDTO>() };
            var tag = new TagDTO { Id = System.Guid.NewGuid(), Name = "Tag1" };
            t.Tags.Add(tag);
            Assert.AreEqual(1, t.Tags.Count);
            Assert.AreEqual("Tag1", t.Tags[0].Name);
        }

        [TestMethod]
        public void TaskDTO_WithMultipleTags_Works()
        {
            var tag1 = new TagDTO { Id = Guid.NewGuid(), Name = "Tag1" };
            var tag2 = new TagDTO { Id = Guid.NewGuid(), Name = "Tag2" };
            var dto = new TaskDTO { Id = 1, Title = "Test", Tags = new System.Collections.Generic.List<TagDTO> { tag1, tag2 } };
            Assert.AreEqual(2, dto.Tags.Count);
            Assert.AreEqual("Tag1", dto.Tags[0].Name);
            Assert.AreEqual("Tag2", dto.Tags[1].Name);
        }

        [TestMethod]
        public void TagDTO_And_TaskDTO_EdgeCases()
        {
            var tag = new TagDTO { Id = Guid.Empty, Name = string.Empty };
            Assert.AreEqual(Guid.Empty, tag.Id);
            Assert.AreEqual(string.Empty, tag.Name);
            var dto = new TaskDTO { Id = 0, Title = null };
            Assert.AreEqual(0, dto.Id);
            Assert.IsNull(dto.Title);
        }
    }
} 