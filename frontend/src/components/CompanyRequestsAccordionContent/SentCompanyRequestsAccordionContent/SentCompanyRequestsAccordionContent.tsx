export interface SentCompanyRequestsAccordionContent {
  company: string;
}

export const SentCompanyRequestsAccordionContent = ({
  company,
}: SentCompanyRequestsAccordionContent) => {
  const contentText =
    'Din forespørsel om tilganger til ' +
    <span>{company}</span> +
    ' er sendt. Du vil få beskjed når de er godkjent.';

  return;
  <div>
    <div>{contentText}</div>
    <div>
      <span>Opprinnelig forespørsel</span>
      <span>Tilganger</span>
      <img></img>
    </div>
  </div>;
};
