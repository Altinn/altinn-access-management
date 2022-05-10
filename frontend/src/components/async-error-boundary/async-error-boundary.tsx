import { Suspense } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { useTranslation } from 'react-i18next';
import { useQueryErrorResetBoundary } from 'react-query';

import ErrorFallback from './error-fallback';

type ErrorBoundaryProps = {
  children?: React.ReactNode;
};

const AsyncErrorBoundary = ({ children }: ErrorBoundaryProps) => {
  const { reset } = useQueryErrorResetBoundary();
  const { t } = useTranslation('basic');

  return (
    <ErrorBoundary FallbackComponent={ErrorFallback} onReset={reset}>
      <Suspense fallback={<>{t('loading')}</>}>{children}</Suspense>
    </ErrorBoundary>
  );
};

export default AsyncErrorBoundary;
