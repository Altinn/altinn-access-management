import i18next from 'i18next';
import { useTranslation } from 'react-i18next';
import { useQuery } from 'react-query';

type Props = {
  children: React.ReactNode;
};

const SuspendLoadLocale = ({ children }: Props) => {
  const { i18n } = useTranslation('common');
  const baseUrl = import.meta.env.BASE_URL;
  const localeFilePath = `${baseUrl}locales/${i18n.language}.json`;
  const localeFileUrl = new URL(localeFilePath, import.meta.url).href;

  useQuery(
    'Locales',
    async () => {
      const data = await fetch(localeFileUrl);
      const resource = await data.json();
      i18next.addResourceBundle(i18n.language, 'common', resource);
    },
    {
      staleTime: Infinity,
      suspense: true,
    },
  );

  return <>{children}</>;
};

export default SuspendLoadLocale;
