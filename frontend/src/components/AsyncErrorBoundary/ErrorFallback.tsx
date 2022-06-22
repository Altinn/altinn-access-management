import { FallbackProps } from 'react-error-boundary';
import { useTranslation } from 'react-i18next';

const ErrorFallback = ({ error, resetErrorBoundary }: FallbackProps) => {
  const { t } = useTranslation('basic');
  const message = typeof error === 'string' ? error : error?.message;

  return (
    <div role="alert">
      <p>{t('error.heading')}</p>
      {message ? <pre>{message}</pre> : null}
      <button onClick={resetErrorBoundary}>{t('error.tryAgain')}</button>
    </div>
  );
};

export default ErrorFallback;
