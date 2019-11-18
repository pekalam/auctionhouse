import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';

export class CategorySelectStep {
  constructor(public selectedMainCategory: CategoryTreeNode, public selectedSubCategory: CategoryTreeNode, public selectedSubCategory2: CategoryTreeNode) { }

}
