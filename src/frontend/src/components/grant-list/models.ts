export type Grant = {
  id: string;
  entity: {
    name: string;
    type: string;
    idNumber: string;
  };
  type: string;
};
