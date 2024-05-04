// using System.Collections;
//
// namespace Flamui.Layouting;
//
// public class FlexContainer : ILayoutable
// {
//     public List<ILayoutable> Children = new();
//
//     public ParentData ParentData { get; set; }
//
//     public FlexibleChildConfig? FlexibleChildConfig => null;
//
//     public Dir Direction;
//     public SizeDefinition;
//
//     private float GetTotalAvailableSize()
//     {
//
//     }
//
//     public BoxSize Layout(BoxConstraint constraint)
//     {
//         float totalAvailableSize = GetTotalAvailableSize();
//
//         float totalFixedSize = 0;
//         float totalPercentage = 0;
//
//         //Loop through inflexible children
//         foreach (var child in Children)
//         {
//             if (child.IsFlexible(out var config))
//             {
//                 totalPercentage += config.Percentage;
//                 continue;
//             }
//
//             var size = child.Layout(BoxConstraint.FromDirection(Direction, 0, float.PositiveInfinity, 0,
//                 constraint.GetCrossAxis(Direction).Max));
//
//             totalFixedSize += size.GetMainAxis(Direction);
//         }
//
//         var sizePerPercentage = GetSizePerPercentage(totalPercentage, 0);
//
//         float totalSizeOfFlexibleChildren = 0;
//
//         //loop through all flexible children and add up size
//         for (var i = 0; i < Children.Count; i++)
//         {
//             var child = Children[i];
//             if (!child.IsFlexible(out var config))
//                 continue;
//
//             totalSizeOfFlexibleChildren += config.Percentage * sizePerPercentage;
//
//             /*
//             if (mainSize > config.Max && mainSize < config.Min)
//             {
//                 var constraintMainSize = Math.Clamp(mainSize, config.Min, config.Max);
//                 totalFixedSize += mainSize;
//                 child.Layout(BoxConstraint.FromDirection(Direction, constraintMainSize, constraintMainSize, 0,
//                     constraint.GetCrossAxis(Direction).Max));
//                 continue;
//             }
//             */
//
//             // childrenThatNeedAnotherPass[i] = true;
//         }
//
//         float remainingSize = constraint.GetMainAxis(Direction).Max - totalFixedSize;
//
//         //if fits, apply constraints
//         if (totalSizeOfFlexibleChildren < remainingSize)
//         {
//             Span<bool> childrenThatNeedAnotherPass = stackalloc bool[Children.Count];
//             float totalPercentage = 0;
//
//             for (var i = 0; i < Children.Count; i++)
//             {
//                 var child = Children[i];
//                 if (!child.IsFlexible(out var config))
//                     continue;
//
//                 var mainSize = config.Percentage * sizePerPercentage;
//
//                 //if doesn't fit, apply constraint
//                 if (mainSize > config.Max || mainSize < config.Min)
//                 {
//                     var constraintMainSize = Math.Clamp(mainSize, config.Min, config.Max);
//                     totalFixedSize += mainSize;
//                     child.Layout(BoxConstraint.FromDirection(Direction, constraintMainSize, constraintMainSize, 0,
//                         constraint.GetCrossAxis(Direction).Max));
//                     continue;
//                 }
//
//                 totalPercentage += config.Percentage;
//                 childrenThatNeedAnotherPass[i] = true;
//             }
//
//             sizePerPercentage =
//                 GetSizePerPercentage(totalPercentage, constraint.GetMainAxis(Direction).Max - totalFixedSize);
//
//             for (var i = 0; i < Children.Count; i++)
//             {
//                 if (!childrenThatNeedAnotherPass[i])
//                     continue;
//
//                 var child = Children[i];
//
//
//             }
//         }
//
//         // sizePerPercentage = GetSizePerPercentage(totalPercentage, constraint.GetCrossAxis(Dir.Horizontal).Max)
//
//
//             var child = Children[i];
//
//
//         }
//
//         return new BoxSize();
//     }
//
//     private float GetSizePerPercentage(float totalPercentage, float availableSize)
//     {
//         float sizePerPercent;
//
//         if (totalPercentage > 100)
//         {
//             sizePerPercent = availableSize / totalPercentage;
//         }
//         else
//         {
//             sizePerPercent = availableSize / 100;
//         }
//
//         return sizePerPercent;
//     }
// }
