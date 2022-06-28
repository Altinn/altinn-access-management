import TrashCanNormal from './TrashCanNormal.svg';

export interface SentCompanyRequestsHeaderAction {
  text: string;
}

export const SentCompanyRequestsHeaderAction = ({
  text,
}: SentCompanyRequestsHeaderAction) => {
  return (
    <div>
      <span>{text}</span>
      <img src={TrashCanNormal}></img>
    </div>
  );
};
