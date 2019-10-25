export enum Condition
{
    Used = 0, New
}

export interface Product {
  name: string;
  description: string;
  condition: Condition;
}
