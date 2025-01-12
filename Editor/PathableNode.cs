﻿using System.Windows.Forms;
using BhModule.Community.Pathing.Entity;

namespace BhModule.Community.Pathing.Editor {
    public class PathableNode : TreeNode {

        public IPathingEntity PathingEntity { get; private set; }

        public PathableNode(IPathingEntity pathingEntity) : base() {
            this.PathingEntity = pathingEntity;

            if (this.PathingEntity is StandardMarker marker) {
                this.SelectedImageKey = this.ImageKey = "marker";
                this.Text     = $"Marker [{marker.Guid}]";
            } else if (this.PathingEntity is StandardTrail trail) {
                this.SelectedImageKey = this.ImageKey = "trail";
                this.Text             = $"Trail";
            }
        }

    }
}
