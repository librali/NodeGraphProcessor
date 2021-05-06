using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor
{
    public class GroupView : UnityEditor.Experimental.GraphView.Group
	{
		public BaseGraphView	owner;
		public BaseGroup		    BaseGroup;

        Label                   titleLabel;
        ColorField              colorField;

        readonly string         groupStyle = "GraphProcessorStyles/GroupView";

        public GroupView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
		}
		
		private static void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
		
		public virtual void Initialize(BaseGraphView graphView, BaseGroup block)
		{
			BaseGroup = block;
			owner = graphView;

            title = block.title;
            SetPosition(block.position);
			
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			
            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField{ value = BaseGroup.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(BaseGroup.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
		}

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in BaseGroup.innerNodeGUIDs.ToList())
            {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);
                    BaseGroup.innerNodeGUIDs.Remove(nodeGUID);
                    continue ;
                }
                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var node = element as BaseNodeView;

                // Adding an element that is not a node currently supported
                if (node == null)
                    continue;

                if (!BaseGroup.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                    BaseGroup.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }
            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            // Only remove the nodes when the group exists in the hierarchy
            if (parent != null)
            {
                foreach (var elem in elements)
                {
                    if (elem is BaseNodeView nodeView)
                    {
                        BaseGroup.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);
                    }
                }
            }

            base.OnElementsRemoved(elements);
        }

        public void UpdateGroupColor(Color newColor)
        {
            BaseGroup.color = newColor;
            style.backgroundColor = newColor;
        }

        void TitleChangedCallback(ChangeEvent< string > e)
        {
            BaseGroup.title = e.newValue;
        }

		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

			BaseGroup.position = newPos;
		}
	}
}