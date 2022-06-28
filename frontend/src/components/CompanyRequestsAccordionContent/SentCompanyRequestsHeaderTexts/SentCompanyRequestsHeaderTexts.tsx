import classes from './SentCompanyRequestsHeaderTexts.module.css';

export interface SentCompanyRequestsHeaderTexts {
  title: string;
  subtitle: string;
}

export const SentCompanyRequestsHeaderTexts = ({
  title,
  subtitle,
}: SentCompanyRequestsHeaderTexts) => {
  return (
    <div className={classes['sent-company-requests-header-texts']}>
      <span>{title}</span>
      <span>{subtitle}</span>
    </div>
  );
};
