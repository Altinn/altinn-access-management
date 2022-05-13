/**
 * Returns a type with all array items passed in except the first one
 * @ see https://stackoverflow.com/questions/63024617/how-to-reference-all-parameters-except-first-in-typescript
 */
export type DropFirst<T extends unknown[]> = T extends [unknown, ...infer U]
  ? U
  : never;
