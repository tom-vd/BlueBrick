﻿// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BlueBrick.MapData;
using System.Drawing;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Items
{
	public abstract class DuplicateItems : Action
	{
		/// <summary>
		/// This static tool method, clone all the item of the specified list into a new list.
		/// This method also clone the groups that may belong to this list of bricks.
		/// The cloned items are in the same order as the original list
		/// </summary>
		/// <param name="listToClone">The original list of brick to copy</param>
		/// <returns>A clone list of cloned brick with there cloned groups</returns>
		protected List<Layer.LayerItem> cloneItemList(List<Layer.LayerItem> listToClone)
		{
			// the resulting list
			List<Layer.LayerItem> result = new List<Layer.LayerItem>(listToClone.Count);

			// use a dictionnary to recreate the groups that may be inside the list of brick to duplicate
			// this dictionnary makes an association between the group to duplicate and the new duplicated one
			Dictionary<Layer.Group, Layer.Group> groupsToCreate = new Dictionary<Layer.Group, Layer.Group>();
			// also use a list of item that we will make grow to create all the groups
			List<Layer.LayerItem> fullOriginalItemList = new List<Layer.LayerItem>(listToClone);

			// use a for instead of a foreach because the list will grow
			for (int i = 0; i < fullOriginalItemList.Count; ++i)
			{
				// get the current item
				Layer.LayerItem originalItem = fullOriginalItemList[i];
				Layer.LayerItem duplicatedItem = null;

				// check if the item is a group or a brick
				if (originalItem.IsAGroup)
				{
					// if the item is a group that means the list already grown, and that means we also have it in the dictionnary
					Layer.Group associatedGroup = null;
					groupsToCreate.TryGetValue(originalItem as Layer.Group, out associatedGroup);
					duplicatedItem = associatedGroup;
				}
				else
				{
					// if the item is a brick, just clone it and add it to the result
					// clone the item (because the same list of text to add can be paste several times)
					duplicatedItem = originalItem.Clone();
					// add the duplicated item in the list
					result.Add(duplicatedItem);
				}

				// check if the item to clone belongs to a group then also duplicate the group
				if (originalItem.Group != null)
				{
					// get the duplicated group if already created otherwise create it and add it in the dictionary
					Layer.Group duplicatedGroup = null;
					groupsToCreate.TryGetValue(originalItem.Group, out duplicatedGroup);
					if (duplicatedGroup == null)
					{
						duplicatedGroup = new Layer.Group(originalItem.Group);
						groupsToCreate.Add(originalItem.Group, duplicatedGroup);
						fullOriginalItemList.Add(originalItem.Group);
					}
					// assign the group to the brick
					duplicatedGroup.addItem(duplicatedItem);
					// check if we need to also assign the brick that hold the connection point
					if (originalItem.Group.BrickThatHoldsActiveConnection == originalItem)
						duplicatedGroup.BrickThatHoldsActiveConnection = (duplicatedItem as LayerBrick.Brick);
				}
			}

			// delete the dictionary
			groupsToCreate.Clear();
			fullOriginalItemList.Clear();
			// return the cloned list
			return result;
		}

		/// <summary>
		/// The duplacate brick action is a bit specific because the position shift of the duplicated
		/// bricks can be updated after the execution of the action. This is due to a combo from the UI.
		/// In the UI of the application by pressing a modifier key + moving the mouse you can duplicate
		/// the selection but also move it at the same moment, but since it is the same action for the user
		/// we don't want to record 2 actions in the undo stack (one for duplicate, another for move)
		/// </summary>
		/// <param name="positionShiftX">the new shift for x coordinate from the position when this action was created</param>
		/// <param name="positionShiftY">the new shift for y coordinate from the position when this action was created</param>
		public virtual void updatePositionShift(float positionShiftX, float positionShiftY)
		{
			// TODO add the mItems list in the base class and remove duplicated code
			//foreach (Layer.LayerItem item in mItems)
			//{
			//    PointF newPosition = item.Position;
			//    newPosition.X += positionShiftX;
			//    newPosition.Y += positionShiftY;
			//}
		}
	}
}
