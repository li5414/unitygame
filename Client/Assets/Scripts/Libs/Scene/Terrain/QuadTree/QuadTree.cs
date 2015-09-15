﻿using System;
using System.Collections.Generic;
using UnityEngine;
using SDK.Lib;

namespace QuadTree
{
    /// <summary>
    /// A Quadtree is a structure designed to partition space so
    /// that it's faster to find out what is inside or outside a given 
    /// area. See http://en.wikipedia.org/wiki/Quadtree
    /// This QuadTree contains items that have an area (Rectangle)
    /// it will store a reference to the item in the quad 
    /// that is just big enough to hold it. Each quad has a bucket that 
    /// contain multiple items.
    /// </summary>
    public class QuadTree<T>
    {
        /// <summary>
        /// The root QuadTreeNode
        /// </summary>
        QuadTreeNode<T> m_root;

        /// <summary>
        /// The bounds of this QuadTree
        /// </summary>
        RectangleF m_rectangle;

        /// <summary>
        /// An delegate that performs an action on a QuadTreeNode
        /// </summary>
        /// <param name="obj"></param>
        public delegate void QTAction(QuadTreeNode<T> obj);
		
		private int nodeCapacity = 10;

		public int NodeCapacity {
			get {
				return this.nodeCapacity;
			}
			set {
				nodeCapacity = value;
			}
		}
		
		private int minNodeSize = 10;

		public int MinNodeSize {
			get {
				return this.minNodeSize;
			}
			set {
				minNodeSize = value;
			}
		}
		
		public Func<T, RectangleF> GetRect {get; set;}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        public QuadTree(RectangleF rectangle)
        {
            m_rectangle = rectangle;
            m_root = new QuadTreeNode<T>(m_rectangle, this);
        }

        /// <summary>
        /// Get the count of items in the QuadTree
        /// </summary>
        public int Count { get { return m_root.Count; } }

        /// <summary>
        /// Insert the feature into the QuadTree
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            m_root.Insert(item, GetRect (item));
        }
		
		public void Remove(T item)
		{
			ForEach ( node => {
				if (node.Contents.Contains (item))
					node.Contents.Remove (item);
			});
		}

        /// <summary>
        /// Query the QuadTree, returning the items that are in the given area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public IEnumerable<T> Query(RectangleF area)
        {
            return m_root.Query(area);
        }
		
		public IEnumerable<QuadTreeNode<T>> Nodes
		{
			get {
				yield return m_root;
				foreach (var r in m_root.Nodes)
					yield return r;
			}
		}
        
        /// <summary>
        /// Do the specified action for each item in the quadtree
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(QTAction action)
        {
            m_root.ForEach(action);
        }

        // 深度优先遍历树
        public void VisitTree(Camera camera)
        {
            m_root.VisitNode(camera);
        }
    }
}