import { FC, useState } from 'react';
import classes from './ResourceRegistry.module.css';
import { TextField, Button } from '@altinn/altinn-design-system';
import { PopoverPanel } from '../Common/General/Panel';
import * as RadixPopover from '@radix-ui/react-popover';
import { ReactComponent as HelpIcon } from './HelpIcon.svg';
import SelectSearch from 'react-select-search';

export interface ResourceRegistryProps {
  id: string;
  title: string;
  organisationNumber: string;
  orgCode: string;
  telephone: string;
  email: string;
  isPartOf: string;
  hasPart: string;
  homepage: string;
  type: string[];
  sector: string[];
}

const ResourceRegistry: FC<ResourceRegistryProps> = () => {
  const idOptions = ['Id 1', 'Id 2', 'Id 3'];
  const titles = [
    { name: 'Title 1', value: 'title1' },
    { name: 'Title 2', value: 'title2' },
    { name: 'Title 3', value: 'title3' },
  ];
  const [open, onOpenChange] = useState(false);

  const resourceFormResult = {
    identifier: '',
    description: {
      nb: '',
    },
    title: {
      nb: '',
    },
    hasCompetentAuthority: {
      organization: '',
      orgCode: '',
    },
    contactPoint: {
      phone: '',
      email: '',
    },
    isPartOf: '',
    hasPart: '',
    homepage: '',
    type: '',
    sector: '',
  };

  const handleIdentifierChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.identifier = event.currentTarget.value;
  };

  const handleDescrptionChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.description.nb = event.currentTarget.value;
  };

  const handleTitleChangeChange = (
    event: React.FormEvent<HTMLInputElement>,
  ) => {
    resourceFormResult.title.nb = event.currentTarget.value;
  };

  const handleOrganizationNumberChange = (
    event: React.FormEvent<HTMLInputElement>,
  ) => {
    resourceFormResult.hasCompetentAuthority.organization =
      event.currentTarget.value;
  };

  const handleOrgCodeChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.hasCompetentAuthority.orgCode =
      event.currentTarget.value;
  };

  const handlePhoneChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.contactPoint.phone = event.currentTarget.value;
  };

  const handleEmailChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.contactPoint.email = event.currentTarget.value;
  };

  const handleIsPartOfChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.isPartOf = event.currentTarget.value;
  };

  const handleHasPartChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.hasPart = event.currentTarget.value;
  };

  const handleHomepageChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.homepage = event.currentTarget.value;
  };

  const handleTypeChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.type = event.currentTarget.value;
  };

  const handleSectorChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.sector = event.currentTarget.value;
  };

  const submit = () => {
    console.log(resourceFormResult);
  };

  const questionMark = (
    <span className={classes['question-icon']}>
      <HelpIcon width={22} height={22}></HelpIcon>
    </span>
  );

  return (
    <div className={classes['resource-registry']}>
      <div className={classes['resource-registry__form']}>
        <div className={classes['button-container']}>
          <h2 className={classes['center']}>Ressursregistrering</h2>
        </div>
        <form>
          <div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Identifikator</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi en formell identifikasjon til tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleIdentifierChange(e)}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Tittel</label>
                <SelectSearch
                  options={titles}
                  value={resourceFormResult.title.nb}
                  placeholder="Select title"
                  search={true}
                />
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Identifikator</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi en formell identifikasjon til tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleIdentifierChange(e)}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Tittel</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi det offisielle navnet på tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleTitleChangeChange(e)}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Organisasjonsnummer</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til den offentlige organisasjonen som
                  har ansvaret for tjenesten, uansett om tjenesten tilbys
                  direkte av den aktuelle offentlige organisasjonen eller er
                  satt bort til andre.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleOrganizationNumberChange(e)}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Organisasjonskode</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til den offentlige organisasjonen som
                  har ansvaret for tjenesten, uansett om tjenesten tilbys
                  direkte av den aktuelle offentlige organisasjonen eller er
                  satt bort til andre.{' '}
                </PopoverPanel>
                <TextField onChange={(e) => handleOrgCodeChange(e)}></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Telefonnummer</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi kontaktpunkt(er) for tjenesten.{' '}
                </PopoverPanel>
                <TextField onChange={(e) => handlePhoneChange(e)}></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Epost</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi kontaktpunkt(er) for tjenesten.{' '}
                </PopoverPanel>
                <TextField onChange={(e) => handleEmailChange(e)}></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Beskrivelse</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi en tekstlig beskrivelse av tjenesten. Denne
                  egenskapen kan gjentas når beskrivelsen finnes i flere språk.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleDescrptionChange(e)}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Hjemmeside</label>
                <PopoverPanel trigger={questionMark}>
                  Hjemmeside til tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleHomepageChange(e)}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Er del av</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til en annen tjeneste som denne
                  tjenesten er en del av.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleIsPartOfChange(e)}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Har part</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til en annen tjeneste som denne
                  tjenesten er en del av.{' '}
                </PopoverPanel>
                <TextField onChange={(e) => handleHasPartChange(e)}></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Type</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å indikere type tjeneste i henhold til et
                  kontrollert vokabular.
                </PopoverPanel>
                <TextField onChange={(e) => handleTypeChange(e)}></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Sektor</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til industri/sektor som den aktuelle
                  offentlige tjenesten er relatert til, eller er ment for. En
                  tjeneste kan relateres til flere industrier/sektorer.
                </PopoverPanel>
                <TextField onChange={(e) => handleSectorChange(e)}></TextField>
              </div>
            </div>
            <div className={classes['resource-registry__submit-button']}>
              <Button onClick={submit}>Registrer</Button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ResourceRegistry;
