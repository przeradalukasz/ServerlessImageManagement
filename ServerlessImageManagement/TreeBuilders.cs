using System;
using ServerlessImageManagement.DTO;
using System.Collections.Generic;
using System.Linq;

namespace ServerlessImageManagement
{
    public static class TreeBuilders
    {
        public static void BuildTree(this TreeElement result, IEnumerable<string> remainingElements, string baseAbsolutePath, string baseRelativePath)
        {
            var elementsList = remainingElements as IList<string> ?? remainingElements.ToList();
            if (elementsList.Any())
            {
                var name = elementsList.First();
                var child = result.Children.SingleOrDefault(x => x.Name == name);
                if (child == null)
                {
                    child = new TreeElement()
                    {
                        Name = name,
                        Children = new List<TreeElement>(),
                        AbsolutePath = baseAbsolutePath + '/' + name,
                        RelativePath = baseRelativePath + '/' + name
                    };
                    result.Children.Add(child);
                }
                BuildTree(child, elementsList.Skip(1), child.AbsolutePath, child.RelativePath);
            }
        }

        public static TreeElement GetHierarchy(this IEnumerable<string> paths, string userId)
        {
            var baseRelativePath = "/" + userId;
            var root = new TreeElement() { Name = userId, Children = new List<TreeElement>(), RelativePath = baseRelativePath };

            foreach (var path in paths)
            {
                var splitBetweenBaseAndDirectory = path.IndexOf(userId, StringComparison.Ordinal) + userId.Length;
                var pathWithoutUser = path.Remove(0, splitBetweenBaseAndDirectory).TrimStart('/');
                if (!string.IsNullOrWhiteSpace(pathWithoutUser))
                {
                    var parts = pathWithoutUser.Split('/');
                    var baseAbsolutePath = path.Substring(0, splitBetweenBaseAndDirectory);
                    root.BuildTree(parts, baseAbsolutePath, baseRelativePath);
                }

            }
            return root;
        }
    }
}
