import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';

export interface CategorySelectStep {
  selectedMainCategory: CategoryTreeNode;
  selectedSubCategory: CategoryTreeNode;
  selectedSubCategory2: CategoryTreeNode;
}
