export interface SentCompanyRequestsHeaderAction {
  text: string;
}

export const SentCompanyRequestsHeaderAction = ({
  text,
}: SentCompanyRequestsHeaderAction) => {
  return (
    <div>
      <span>{text}</span>
    </div>
  );
};
