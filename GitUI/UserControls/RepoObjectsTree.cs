﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : UserControl
    {
        /// <summary>the <see cref="GitModule"/></summary>
        GitModule git;
        GitUICommands uiCommands;

        TreeNode nodeTags;
        TreeNode nodeBranches;
        TreeNode nodeStashes;

        List<RepoObjectsTreeSet> treeSets = new List<RepoObjectsTreeSet>();

        public RepoObjectsTree()
        {
            InitializeComponent();

            treeMain.ShowNodeToolTips = true;

            nodeBranches = GetNode("branches");
            nodeTags = GetNode("tags");
            nodeStashes = GetNode("stashes");

            //AddTreeSet(new EasyRepoTreeSet<BranchNode>(git,nodeBranches,));
            AddTreeSet(nodeStashes, (git) => git.GetStashes(), ReloadStashes, AddStash);

            foreach (TreeNode node in treeMain.Nodes)
            {
                ApplyTreeNodeStyle(node);
            }
        }

        void AddTreeSet<T>(
            TreeNode rootNode,
            Func<GitModule, ICollection<T>> getValues,
            Action<ICollection<T>> onReset,
            Func<TreeNodeCollection, T, TreeNode> onAddChild)
        {
            AddTreeSet(new EasyRepoTreeSet<T>(git, rootNode, getValues, onReset, onAddChild));
        }

        void AddTreeSet(RepoObjectsTreeSet treeSet)
        {
            treeSets.Add(treeSet);
        }

        TreeNode GetNode(string node)
        {
            return treeMain.Nodes.Find(node, false)[0];
        }

        /// <summary>Sets up the objects tree for a new repo, then reloads the objects tree.</summary>
        public void NewRepo(GitModule git, GitUICommands uiCommands)
        {
            this.git = git;
            this.uiCommands = uiCommands;

            foreach (RepoObjectsTreeSet treeSet in treeSets)
            {
                treeSet.NewRepo(git);
            }

            Reload();
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload()
        {
            // todo: async CancellationToken(s)
            // todo: task exception handling

            foreach (RepoObjectsTreeSet treeSet in treeSets)
            {
                treeSet.ReloadAsync();
            }

            // update tree little by little OR when all data retrieved?

            //Task.Factory.ContinueWhenAll(
            //    new[] { taskBranches },
            //    tasks => treeMain.EndUpdate(),
            //    new CancellationToken(),
            //    TaskContinuationOptions.NotOnCanceled,
            //    uiScheduler);
        }

        /// <summary>Applies the style to the specified <see cref="TreeNode"/>.
        /// <remarks>Should be invoked from a more specific style.</remarks></summary>
        static void ApplyTreeNodeStyle(TreeNode node)
        {
            node.NodeFont = Settings.Font;
            // ...
        }

        void ExpandAll_Click(object sender, EventArgs e)
        {
            treeMain.ExpandAll();
        }

        void CollapseAll_Click(object sender, EventArgs e)
        {
            treeMain.CollapseAll();
        }

        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {

        }

        /// <summary>Performed on a <see cref="TreeNode"/> double-click.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.IsAncestorOf(nodeBranches))
            {// branches/
                if (node.HasNoChildren())
                {// no children -> branch
                    // needs to go into Settings, but would probably like an option to:
                    // stash; checkout;
                    uiCommands.StartCheckoutBranchDialog(base.ParentForm, node.Text, false);
                }

            }
        }
    }

}
