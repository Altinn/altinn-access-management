import { ErrorBoundary, FallbackProps } from 'react-error-boundary';
import { useQueryErrorResetBoundary } from 'react-query';

const ErrorFallback = ({ error, resetErrorBoundary }: FallbackProps) => {
  return (
    <div role="alert">
      <p>Something went wrong:</p>
      <pre>{error.message}</pre>
      <button onClick={resetErrorBoundary}>Try again</button>
    </div>
  );
};

type ErrorBoundaryProps = {
  children?: React.ReactNode;
  isLoading?: boolean;
};

const AsyncLoader = ({ children, isLoading }: ErrorBoundaryProps) => {
  const { reset } = useQueryErrorResetBoundary();

  if (isLoading) {
    return <>Loading…'</>;
  }

  return (
    <ErrorBoundary FallbackComponent={ErrorFallback} onReset={reset}>
      {children}
    </ErrorBoundary>
  );
};

export default AsyncLoader;
