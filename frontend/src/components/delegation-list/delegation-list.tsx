import { DelegationRequest } from './models';

export interface DelegationListProps {
  delegations?: DelegationRequest[];
}

const DelegationList = ({ delegations }: DelegationListProps) => {
  if (!delegations || delegations.length === 0) {
    return <>Nothing to see here…</>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Created</th>
          <th>Modified</th>
          <th>Covered by</th>
          <th>Offered by</th>
        </tr>
      </thead>
      <tbody>
        {delegations.map((delegation) => (
          <tr key={delegation.guid}>
            <td>{delegation.created}</td>
            <td>{delegation.lastChanged}</td>
            <td>{delegation.coveredBy}</td>
            <td>{delegation.offeredBy}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default DelegationList;
