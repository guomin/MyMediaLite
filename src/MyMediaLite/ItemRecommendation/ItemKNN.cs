// Copyright (C) 2013 João Vinagre, Zeno Gantner
// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>k-nearest neighbor (kNN) item-based collaborative filtering</summary>
	public class ItemKNN : KNN, IItemSimilarityProvider
	{
		///
		protected override EntityType Entity { get { return EntityType.ITEM; } }

		///
		public override void Train()
		{
			base.Train();

			int num_items = MaxItemID + 1;
			if (k != uint.MaxValue)
			{
				this.nearest_neighbors = new List<IList<int>>(num_items);
				for (int i = 0; i < num_items; i++)
					nearest_neighbors.Add(correlation_matrix.GetNearestNeighbors(i, k));
			}
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				return float.MinValue;
			if (item_id > MaxItemID)
				return float.MinValue;

			if (k != uint.MaxValue)
			{
				double sum = 0;
				double normalization = 0;
				if (nearest_neighbors[item_id] != null)
				{
					foreach (int neighbor in nearest_neighbors[item_id])
					{
						normalization += Math.Pow(correlation_matrix[item_id, neighbor], Q);
						if (Feedback.ItemMatrix[neighbor, user_id])
							sum += Math.Pow(correlation_matrix[item_id, neighbor], Q);
					}
				}
				if (sum == 0) return 0;
				return (float) (sum / normalization);
			}
			else
			{
				// roughly 10x faster
				// TODO: implement normalization
				return (float) correlation_matrix.SumUp(item_id, Feedback.UserMatrix[user_id], Q);
			}
		}

		///
		public float GetItemSimilarity(int item_id1, int item_id2)
		{
			return correlation_matrix[item_id1, item_id2];
		}

		///
		public IList<int> GetMostSimilarItems(int item_id, uint n = 10)
		{
			if (n <= k)
				return nearest_neighbors[item_id].Take((int) n).ToArray();
			else
				return correlation_matrix.GetNearestNeighbors(item_id, n);
		}
	}
}
